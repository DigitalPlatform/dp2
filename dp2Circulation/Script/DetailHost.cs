#define SHITOUTANG  // 石头汤分类法和著者号的支持

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;
using System.IO;

using DigitalPlatform;
using DigitalPlatform.Marc;
using DigitalPlatform.CommonControl;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using DigitalPlatform.Script;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;

using DigitalPlatform.GcatClient;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;

namespace dp2Circulation
{
    /// <summary>
    /// 详细窗二次开发宿主类
    /// 替代了以前的 Host 类
    /// </summary>
    public class DetailHost : IDetailHost
    {
        ScriptActionCollection _scriptActions = new ScriptActionCollection();
        IBiblioItemsWindow _detailWindow = null;

        #region IDetailHost 接口要求

        public Form Form
        {
            get
            {
                return (this.DetailForm as Form);
            }
            set
            {
                this.DetailForm = (value as EntityForm);
            }
        }

        /// <summary>
        /// 种册窗
        /// </summary>
        public IBiblioItemsWindow DetailWindow
        {
            get
            {
                return this._detailWindow;
            }
            set
            {
                this._detailWindow = value;
            }
        }

        /// <summary>
        /// 脚本编译后的 Assembly
        /// </summary>
        public Assembly Assembly
        {
            get;
            set;
        }

        /// <summary>
        /// Ctrl+A 功能名称的集合
        /// </summary>
        public ScriptActionCollection ScriptActions
        {
            get
            {
                return _scriptActions;
            }
            set
            {
                _scriptActions = value;
            }
        }

        /// <summary>
        /// 调用一个 Ctrl+A 功能
        /// </summary>
        /// <param name="strFuncName">功能名</param>
        /// <param name="sender">调用者</param>
        /// <param name="e">Ctrl+A 事件参数</param>
        public void Invoke(string strFuncName,
            object sender,
            // GenerateDataEventArgs e
            EventArgs e)
        {
            Type classType = this.GetType();

            while (classType != null)
            {
                try
                {
                    // 有两个参数的成员函数
                    classType.InvokeMember(strFuncName,
                        BindingFlags.DeclaredOnly |
                        BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.Instance | BindingFlags.InvokeMethod
                        ,
                        null,
                        this,
                        new object[] { sender, e });
                    return;

                }
                catch (System.MissingMethodException/*ex*/)
                {
                    classType = classType.BaseType;
                    if (classType == null)
                        break;
                }
            }

            classType = this.GetType();

            while (classType != null)
            {
                try
                {
                    // 兼容以前的书写方法 -- 没有参数
                    classType.InvokeMember(strFuncName,
                        BindingFlags.DeclaredOnly |
                        BindingFlags.Public | BindingFlags.NonPublic |
                        BindingFlags.Instance | BindingFlags.InvokeMethod
                        ,
                        null,
                        this,
                        null);
                    return;
                }
                catch (System.MissingMethodException/*ex*/)
                {
                    classType = classType.BaseType;
                }
            }

            throw new Exception("函数 void " + strFuncName + "(object sender, GenerateDataEventArgs e) 或 void " + strFuncName + "() 没有找到");
        }

        public virtual void CreateMenu(object sender, GenerateDataEventArgs e)
        {
            ScriptActionCollection actions = new ScriptActionCollection();

            if (sender is MarcEditor || sender == null)
            {
#if TESTING
            actions.NewItem("调试用", "调试用", "Test", false);
#endif

#if NO
                // 规整ISBN为13
                actions.NewItem("规整为ISBN-13", "对010$a中ISBN进行规整", "HyphenISBN_13", false);

                // 规整ISBN为10
                actions.NewItem("规整为ISBN-10", "对010$a中ISBN进行规整", "HyphenISBN_10", false);
#endif
            }

            if (sender is BinaryResControl || sender is MarcEditor)
            {
                // 856字段
                actions.NewItem("创建维护856字段", "创建维护856字段", "Manage856", false);
            }

            if (sender is EntityEditForm || sender is EntityControl || sender is BindingForm)
            {
                // 创建索取号
                actions.NewItem("创建索取号", "为册记录创建索取号", "CreateCallNumber", false);

                // 管理索取号
                actions.NewItem("管理索取号", "为册记录管理索取号", "ManageCallNumber", false);
            }

            this.ScriptActions = actions;
        }

        // 设置菜单加亮状态 -- 856字段
        void Manage856_setMenu(object sender, SetMenuEventArgs e)
        {
            Field curfield = this.DetailForm.MarcEditor.FocusedField;
            if (curfield != null && curfield.Name == "856")
                e.Action.Active = true;
            else
                e.Action.Active = false;
        }

        // 设置菜单加亮状态 -- 创建索取号
        void CreateCallNumber_setMenu(object sender, SetMenuEventArgs e)
        {
            e.Action.Active = false;
            if (e.sender is EntityEditForm)
                e.Action.Active = true;
        }

        #endregion

        // 引用
        /// <summary>
        /// 种册窗
        /// </summary>
        public EntityForm DetailForm = null;

        /// <summary>
        /// GCAT 通讯通道
        /// </summary>
        DigitalPlatform.GcatClient.Channel GcatChannel = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public DetailHost()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public void Dispose()
        {
            if (this.GcatChannel != null)
                this.GcatChannel.Dispose();

            // 2017/4/23
            if (this.DetailForm != null)
                this.DetailForm = null;
        }

        /// <summary>
        /// 入口函数
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        public virtual void Main(object sender, GenerateDataEventArgs e/*HostEventArgs e*/)
        {

        }

        /// <summary>
        /// 调用一个 Ctrl+A 功能
        /// </summary>
        /// <param name="strFuncName">功能名</param>
        public void Invoke(string strFuncName)
        {
            Type classType = this.GetType();

            // 调用成员函数
            classType.InvokeMember(strFuncName,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.InvokeMethod
                ,
                null,
                this,
                null);
        }

        // 数据保存前的处理工作
        /// <summary>
        /// 数据保存前的处理工作
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        public virtual void BeforeSaveRecord(object sender,
            BeforeSaveRecordEventArgs e)
        {
            if (sender == null)
                return;

            int nRet = 0;
            string strError = "";
            bool bChanged = false;

            try
            {
                // 对MARC记录进行处理
                if (sender is MarcEditor)
                {
                    // 编目批次号
                    string strBatchNo = this.GetFirstSubfield("998", "a");
                    if (string.IsNullOrEmpty(strBatchNo) == true)
                    {
                        string strValue = "";
                        // 检查本地 %catalog_batchno% 宏是否存在
                        // 从marceditor_macrotable.xml文件中解析宏
                        // return:
                        //      -1  error
                        //      0   not found
                        //      1   found
                        nRet = MacroUtil.GetFromLocalMacroTable(
                            // PathUtil.MergePath(Program.MainForm.DataDir, "marceditor_macrotable.xml"),
                            PathUtil.MergePath(Program.MainForm.UserDir, "marceditor_macrotable.xml"),
                "catalog_batchno",
                false,
                out strValue,
                out strError);
                        if (nRet == -1)
                        {
                            e.ErrorInfo = strError;
                            return;
                        }
                        if (nRet == 1 && string.IsNullOrEmpty(strValue) == false)
                        {
                            this.SetFirstSubfield("998", "a", strValue);
                            bChanged = true;
                        }
                    }

                    // 记录创建时间
                    string strCreateTime = this.GetFirstSubfield("998", "u");
                    if (string.IsNullOrEmpty(strCreateTime) == true)
                    {
                        DateTime now = DateTime.Now;
                        strCreateTime = now.ToString("u");
                        this.SetFirstSubfield("998", "u", strCreateTime);
                        bChanged = true;
                    }

                    // 记录创建者
                    string strCreator = this.GetFirstSubfield("998", "z");
                    if (string.IsNullOrEmpty(strCreator) == true)
                    {
                        strCreator = this.DetailForm.CurrentUserName;   // this.DetailForm.Channel.UserName;
                        this.SetFirstSubfield("998", "z", strCreator);
                        bChanged = true;
                    }

                    e.Changed = bChanged;
                }
            }
            catch (Exception ex)
            {
                e.ErrorInfo = ex.Message;
            }
        }

        // 验收创建册记录后的处理工作
        /// <summary>
        /// 验收创建册记录后的处理工作
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        public virtual void AfterCreateItems(object sender,
            AfterCreateItemsArgs e)
        {
            if (sender == null)
                return;
#if NO
            string strError = "";
            string strHtml = "";
            // 汇总已经推荐过本书目的评注信息，HTML格式
            int nRet = this.DetailForm.CommentControl.GetOrderSuggestionHtml(
                out strHtml,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            if (string.IsNullOrEmpty(strHtml) == true)
                return;

            strHtml = "<html><head>"
                +"<style media='screen' type='text/css'>"
                +"body, input, select { FONT-FAMILY: Microsoft YaHei, Verdana, 宋体; }"
                +"body { padding: 20px;	background-color: #White; }"
                + "table { width: 100%; font-size: 12pt; border-style: solid; border-width: 1pt; border-color: #000000;	border-collapse:collapse; border-width: 1pt; } "
                + "table td { padding : 8px; border-style: dotted; border-width: 1pt; border-color: #555555; } "
                + "table tr.column td { color: White; background-color: #999999; font-weight: bolder; } "
                + "</style>"
                +"</head>"
                +"<body>" + strHtml + "</body></html>";

            HtmlViewerForm dlg = new HtmlViewerForm();
            dlg.Text = "荐购者信息";
            dlg.HtmlString = strHtml;
            Program.MainForm.AppInfo.LinkFormState(dlg, "AfterCreateItems_dialog_state");
            dlg.ShowDialog(this.DetailForm);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            return;
        ERROR1:
            MessageBox.Show(this.DetailForm, strError);
#endif
        }


        // parameters:
        //      strIndicator    字段指示符。如果用null调用，则表示不对指示符进行筛选
        // return:
        //      0   没有找到匹配的配置事项
        //      >=1 找到。返回找到的配置事项个数
        /// <summary>
        /// 获得和一个字段相关的拼音配置事项集合
        /// </summary>
        /// <param name="cfg_dom">存储了配置信息的 XmlDocument 对象</param>
        /// <param name="strFieldName">字段名</param>
        /// <param name="strIndicator">字段指示符</param>
        /// <param name="cfg_items">返回匹配的配置事项集合</param>
        /// <returns>0: 没有找到匹配的配置事项; >=1: 找到。值为配置事项个数</returns>
        public static int GetPinyinCfgLine(XmlDocument cfg_dom,
            string strFieldName,
            string strIndicator,
            out List<PinyinCfgItem> cfg_items)
        {
            cfg_items = new List<PinyinCfgItem>();

            XmlNodeList nodes = cfg_dom.DocumentElement.SelectNodes("item");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                PinyinCfgItem item = new PinyinCfgItem(node);

                if (item.FieldName != strFieldName)
                    continue;

                if (string.IsNullOrEmpty(item.IndicatorMatchCase) == false
                    && string.IsNullOrEmpty(strIndicator) == false)
                {
                    if (MarcUtil.MatchIndicator(item.IndicatorMatchCase, strIndicator) == false)
                        continue;
                }

                cfg_items.Add(item);
            }

            return cfg_items.Count;
        }

        // parameters:
        //      strPrefix   要加入拼音子字段内容前部的前缀字符串。例如 {cr:NLC} 或 {cr:CALIS}
        // return:
        //      -1  出错。包括中断的情况
        //      0   正常
        /// <summary>
        /// 为 MARC 编辑器内的记录加拼音
        /// </summary>
        /// <param name="strCfgXml">拼音配置 XML</param>
        /// <param name="bUseCache">是否使用记录中以前缓存的结果？</param>
        /// <param name="style">风格</param>
        /// <param name="strPrefix">前缀字符串。缺省为空</param>
        /// <param name="bAutoSel">是否自动选择多音字</param>
        /// <returns>-1: 出错。包括中断的情况; 0: 正常</returns>
        public virtual int AddPinyin(string strCfgXml,
            bool bUseCache = true,
            PinyinStyle style = PinyinStyle.None,
            string strPrefix = "",
            bool bAutoSel = false)
        {
            string strError = "";
            XmlDocument cfg_dom = new XmlDocument();
            try
            {
                cfg_dom.LoadXml(strCfgXml);
            }
            catch (Exception ex)
            {
                strError = "strCfgXml装载到XMLDOM时出错: " + ex.Message;
                goto ERROR1;
            }

            string strRuleParam = "";
            if (string.IsNullOrEmpty(strPrefix) == false)
            {
                string strCmd = StringUtil.GetLeadingCommand(strPrefix);
                if (string.IsNullOrEmpty(strCmd) == false
&& StringUtil.HasHead(strCmd, "cr:") == true)
                {
                    strRuleParam = strCmd.Substring(3);
                }
            }

            this.DetailForm.MarcEditor.Enabled = false;

            Hashtable old_selected = (bUseCache == true) ? this.DetailForm.GetSelectedPinyin() : new Hashtable();
            Hashtable new_selected = new Hashtable();

            try
            {
                // PinyinStyle style = PinyinStyle.None;	// 在这里修改拼音大小写风格

                for (int i = 0; i < DetailForm.MarcEditor.Record.Fields.Count; i++)
                {
                    Field field = DetailForm.MarcEditor.Record.Fields[i];

                    List<PinyinCfgItem> cfg_items = null;
                    int nRet = GetPinyinCfgLine(
                        cfg_dom,
                        field.Name,
                        field.Indicator,
                        out cfg_items);
                    if (nRet <= 0)
                        continue;

                    string strHanzi = "";
                    string strNextSubfieldName = "";

                    string strField = field.Text;


                    string strFieldPrefix = "";

                    // 2012/11/5
                    // 观察字段内容前面的 {} 部分
                    {
                        string strCmd = StringUtil.GetLeadingCommand(field.Value);
                        if (string.IsNullOrEmpty(strRuleParam) == false
                            && string.IsNullOrEmpty(strCmd) == false
                            && StringUtil.HasHead(strCmd, "cr:") == true)
                        {
                            string strCurRule = strCmd.Substring(3);
                            if (strCurRule != strRuleParam)
                                continue;
                        }
                        else if (string.IsNullOrEmpty(strCmd) == false)
                        {
                            strFieldPrefix = "{" + strCmd + "}";
                        }
                    }

                    // 2012/11/5
                    // 观察 $* 子字段
                    {
                        //
                        string strSubfield = "";
                        string strNextSubfieldName1 = "";
                        // return:
                        //		-1	出错
                        //		0	所指定的子字段没有找到
                        //		1	找到。找到的子字段返回在strSubfield参数中
                        nRet = MarcUtil.GetSubfield(strField,
                            ItemType.Field,
                            "*",    // "*",
                            0,
                            out strSubfield,
                            out strNextSubfieldName1);
                        if (nRet == 1)
                        {
                            string strCurStyle = strSubfield.Substring(1);
                            if (string.IsNullOrEmpty(strRuleParam) == false
                                && strCurStyle != strRuleParam)
                                continue;
                            else if (string.IsNullOrEmpty(strCurStyle) == false)
                            {
                                strFieldPrefix = "{cr:" + strCurStyle + "}";
                            }
                        }
                    }

                    foreach (PinyinCfgItem item in cfg_items)
                    {
                        for (int k = 0; k < item.From.Length; k++)
                        {
                            if (item.From.Length != item.To.Length)
                            {
                                strError = "配置事项 fieldname='" + item.FieldName + "' from='" + item.From + "' to='" + item.To + "' 其中from和to参数值的字符数不等";
                                goto ERROR1;
                            }

                            string from = new string(item.From[k], 1);
                            string to = new string(item.To[k], 1);
                            for (int j = 0; ; j++)
                            {

                                // return:
                                //		-1	error
                                //		0	not found
                                //		1	found

                                nRet = MarcUtil.GetSubfield(strField,
                                    ItemType.Field,
                                    from,
                                    j,
                                    out strHanzi,
                                    out strNextSubfieldName);
                                if (nRet != 1)
                                    break;
                                if (strHanzi.Length <= 1)
                                    break;

                                strHanzi = strHanzi.Substring(1);

                                // 2013/6/13
                                if (DetailHost.ContainHanzi(strHanzi) == false)
                                    continue;

                                string strSubfieldPrefix = "";  // 当前子字段内容本来具有的前缀

                                // 检查内容前部可能出现的 {} 符号
                                string strCmd = StringUtil.GetLeadingCommand(strHanzi);
                                if (string.IsNullOrEmpty(strRuleParam) == false
                                    && string.IsNullOrEmpty(strCmd) == false
                                    && StringUtil.HasHead(strCmd, "cr:") == true)
                                {
                                    string strCurRule = strCmd.Substring(3);
                                    if (strCurRule != strRuleParam)
                                        continue;   // 当前子字段属于和strPrefix表示的不同的编目规则，需要跳过，不给加拼音
                                    strHanzi = strHanzi.Substring(strPrefix.Length); // 去掉 {} 部分
                                }
                                else if (string.IsNullOrEmpty(strCmd) == false)
                                {
                                    strHanzi = strHanzi.Substring(strCmd.Length + 2); // 去掉 {} 部分
                                    strSubfieldPrefix = "{" + strCmd + "}";
                                }

                                string strPinyin = "";

                                strPinyin = (string)old_selected[strHanzi];
                                if (string.IsNullOrEmpty(strPinyin) == true)
                                {
#if NO
                                    // 把字符串中的汉字和拼音分离
                                    // return:
                                    //      -1  出错
                                    //      0   用户希望中断
                                    //      1   正常
                                    if (string.IsNullOrEmpty(Program.MainForm.PinyinServerUrl) == true
                                       || Program.MainForm.ForceUseLocalPinyinFunc == true)
                                    {
                                        nRet = Program.MainForm.HanziTextToPinyin(
                                            this.DetailForm,
                                            true,	// 本地，快速
                                            strHanzi,
                                            style,
                                            out strPinyin,
                                            out strError);
                                    }
                                    else
                                    {
                                        // 汉字字符串转换为拼音
                                        // 如果函数中已经MessageBox报错，则strError第一字符会为空格
                                        // return:
                                        //      -1  出错
                                        //      0   用户希望中断
                                        //      1   正常
                                        //      2   结果字符串中有没有找到拼音的汉字
                                        nRet = Program.MainForm.SmartHanziTextToPinyin(
                                            this.DetailForm,
                                            strHanzi,
                                            style,
                                            bAutoSel,
                                            out strPinyin,
                                            out strError);
                                    }
#endif
                                    nRet = Program.MainForm.GetPinyin(
                                        this.DetailForm,
                                        strHanzi,
                                        style,
                                        bAutoSel,
                                        out strPinyin,
                                        out strError);
                                    if (nRet == -1)
                                    {
                                        new_selected = null;
                                        goto ERROR1;
                                    }
                                    if (nRet == 0)
                                    {
                                        new_selected = null;
                                        strError = "用户中断。拼音子字段内容可能不完整。";
                                        goto ERROR1;
                                    }
                                }

                                if (new_selected != null && nRet != 2)
                                    new_selected[strHanzi] = strPinyin;

                                nRet = MarcUtil.DeleteSubfield(
                                    ref strField,
                                    to,
                                    j);

                                string strContent = strPinyin;

                                if (string.IsNullOrEmpty(strPrefix) == false)
                                    strContent = strPrefix + strPinyin;
                                else if (string.IsNullOrEmpty(strSubfieldPrefix) == false)
                                    strContent = strSubfieldPrefix + strPinyin;
                                /*
                            else if (string.IsNullOrEmpty(strFieldPrefix) == false)
                                strContent = strFieldPrefix + strPinyin;
                                 * */

                                nRet = MarcUtil.InsertSubfield(
                                    ref strField,
                                    from,
                                    j,
                                    new string(MarcUtil.SUBFLD, 1) + to + strContent,
                                    1);
                                field.Text = strField;
                            }
                        }
                    }
                }

                if (new_selected != null)
                    this.DetailForm.SetSelectedPinyin(new_selected);
            }
            finally
            {
                this.DetailForm.MarcEditor.Enabled = true;
                this.DetailForm.MarcEditor.Focus();
            }
            return 0;
            ERROR1:
            if (string.IsNullOrEmpty(strError) == false)
            {
                if (strError[0] != ' ')
                    MessageBox.Show(this.DetailForm, strError);
            }
            return -1;
        }

        /// <summary>
        /// 为 MARC 编辑器内的记录删除拼音
        /// </summary>
        /// <param name="strCfgXml">拼音配置 XML</param>
        /// <param name="strPrefix">前缀字符串。缺省为空</param>
        public virtual void RemovePinyin(string strCfgXml,
            string strPrefix = "")
        {
            string strError = "";
            XmlDocument cfg_dom = new XmlDocument();
            try
            {
                cfg_dom.LoadXml(strCfgXml);
            }
            catch (Exception ex)
            {
                strError = "strCfgXml装载到XMLDOM时出错: " + ex.Message;
                goto ERROR1;
            }

            string strRuleParam = "";
            if (string.IsNullOrEmpty(strPrefix) == false)
            {
                string strCmd = StringUtil.GetLeadingCommand(strPrefix);
                if (string.IsNullOrEmpty(strCmd) == false
&& StringUtil.HasHead(strCmd, "cr:") == true)
                {
                    strRuleParam = strCmd.Substring(3);
                }
            }

            this.DetailForm.MarcEditor.Enabled = false;

            try
            {
                for (int i = 0; i < DetailForm.MarcEditor.Record.Fields.Count; i++)
                {
                    Field field = DetailForm.MarcEditor.Record.Fields[i];

                    List<PinyinCfgItem> cfg_items = null;
                    int nRet = GetPinyinCfgLine(
                        cfg_dom,
                        field.Name,
                        field.Indicator,    // TODO: 可以不考虑指示符的情况，扩大删除的搜寻范围
                        out cfg_items);
                    if (nRet <= 0)
                        continue;

                    string strField = field.Text;

                    // 2012/11/6
                    // 观察字段内容前面的 {} 部分
                    if (string.IsNullOrEmpty(strRuleParam) == false)
                    {
                        string strCmd = StringUtil.GetLeadingCommand(field.Value);
                        if (string.IsNullOrEmpty(strRuleParam) == false
                            && string.IsNullOrEmpty(strCmd) == false
                            && StringUtil.HasHead(strCmd, "cr:") == true)
                        {
                            string strCurRule = strCmd.Substring(3);
                            if (strCurRule != strRuleParam)
                                continue;
                        }
                    }

                    // 2012/11/6
                    // 观察 $* 子字段
                    if (string.IsNullOrEmpty(strRuleParam) == false)
                    {
                        //
                        string strSubfield = "";
                        string strNextSubfieldName1 = "";
                        // return:
                        //		-1	出错
                        //		0	所指定的子字段没有找到
                        //		1	找到。找到的子字段返回在strSubfield参数中
                        nRet = MarcUtil.GetSubfield(strField,
                            ItemType.Field,
                            "*",    // "*",
                            0,
                            out strSubfield,
                            out strNextSubfieldName1);
                        if (nRet == 1)
                        {
                            string strCurStyle = strSubfield.Substring(1);
                            if (string.IsNullOrEmpty(strRuleParam) == false
                                && strCurStyle != strRuleParam)
                                continue;
                        }
                    }

                    bool bChanged = false;
                    foreach (PinyinCfgItem item in cfg_items)
                    {
                        for (int k = 0; k < item.To.Length; k++)
                        {
                            string to = new string(item.To[k], 1);
                            if (string.IsNullOrEmpty(strPrefix) == true)
                            {
                                for (; ; )
                                {
                                    // 删除一个子字段
                                    // 其实原来的ReplaceSubfield()也可以当作删除来使用
                                    // return:
                                    //      -1  出错
                                    //      0   没有找到子字段
                                    //      1   找到并删除
                                    nRet = MarcUtil.DeleteSubfield(
                                        ref strField,
                                        to,
                                        0);
                                    if (nRet != 1)
                                        break;
                                    bChanged = true;
                                }
                            }
                            else
                            {
                                // 只删除具有特定前缀的内容的子字段
                                int nDeleteIndex = 0;
                                for (; ; )
                                {
                                    // 删除前要观察拼音子字段的内容
                                    bool bDelete = false;
                                    string strContent = "";
                                    string strNextSubfieldName = "";
                                    // return:
                                    //		-1	error
                                    //		0	not found
                                    //		1	found
                                    nRet = MarcUtil.GetSubfield(strField,
                                        ItemType.Field,
                                        to,
                                        nDeleteIndex,
                                        out strContent,
                                        out strNextSubfieldName);
                                    if (nRet != 1)
                                        break;
                                    if (strContent.Length <= 1)
                                        bDelete = true; // 空内容的子字段要删除
                                    else
                                    {
                                        strContent = strContent.Substring(1);
                                        if (StringUtil.HasHead(strContent, strPrefix) == true)
                                            bDelete = true;
                                    }

                                    if (bDelete == false)
                                    {
                                        nDeleteIndex++; // 继续处理同名子字段后面的实例
                                        continue;
                                    }

                                    // 删除一个子字段
                                    // 其实原来的ReplaceSubfield()也可以当作删除来使用
                                    // return:
                                    //      -1  出错
                                    //      0   没有找到子字段
                                    //      1   找到并删除
                                    nRet = MarcUtil.DeleteSubfield(
                                        ref strField,
                                        to,
                                        nDeleteIndex);
                                    if (nRet != 1)
                                        break;
                                    bChanged = true;
                                }
                            }
                        }
                    }
                    if (bChanged == true)
                        field.Text = strField;
                }
            }
            finally
            {
                this.DetailForm.MarcEditor.Enabled = true;
                this.DetailForm.MarcEditor.Focus();
            }
            return;
            ERROR1:
            MessageBox.Show(this.DetailForm, strError);
        }
        /*
        // 获得一个子字段
        // 第一个某字段的第一个某子字段
        // return:
        //	-1	error
        //	0	not found
        //	1	succeed
        public static int GetFirstSubfield(
            MarcEditor MarcEditor,
            string strFieldName,
            string strSubfieldName,
            out string strValue,
            out string strError)
        {
            strError = "";
            strValue = "";

            Field field = MarcEditor.Record.Fields.GetOneField(strFieldName, 0);
            SubfieldCollection subfields = field.Subfields;

            Subfield subfield = subfields[strSubfieldName];

            if (subfield == null)
            {
                strError = "MARC子字段 " + strFieldName + "$" + strSubfieldName + " 不存在";
                return 0;
            }

            strValue = subfield.Value;
            return 1;
        }
         * */

        // 找到第一个子字段内容
        // parameters:
        //      location    字符串数组。每个元素为4字符。前三个字符为字段名，最后一个字符为子字段名
        /// <summary>
        /// 找到第一个子字段内容。找到第一个匹配的非空子字段内容就返回
        /// </summary>
        /// <param name="location">字符串数组。集合中每个元素为 4 字符。前三个字符为字段名，最后一个字符为子字段名</param>
        /// <returns>子字段正文内容</returns>
        public string GetFirstSubfield(List<string> location)
        {
            for (int i = 0; i < location.Count; i++)
            {
                string strFieldName = location[i].Substring(0, 3);
                string strSubfieldName = location[i].Substring(3, 1);
                string strValue = this.DetailForm.MarcEditor.Record.Fields.GetFirstSubfield(
                    strFieldName,
                    strSubfieldName);
                if (String.IsNullOrEmpty(strValue) == false)
                    return strValue;
            }

            return null;
        }

        /// <summary>
        /// 获得第一个子字段内容
        /// </summary>
        /// <param name="strFieldName">字段名</param>
        /// <param name="strSubfieldName">子字段名</param>
        /// <returns>子字段正文内容</returns>
        public string GetFirstSubfield(string strFieldName,
            string strSubfieldName)
        {
            return this.DetailForm.MarcEditor.Record.Fields.GetFirstSubfield(
                    strFieldName,
                    strSubfieldName);
        }

        /// <summary>
        /// 设置第一个子字段的正文内容
        /// </summary>
        /// <param name="strFieldName">字段名</param>
        /// <param name="strSubfieldName">子字段名</param>
        /// <param name="strSubfieldValue">要设置的正文字符串</param>
        public void SetFirstSubfield(string strFieldName,
            string strSubfieldName,
            string strSubfieldValue)
        {
            this.DetailForm.MarcEditor.Record.Fields.SetFirstSubfield(
                    strFieldName,
                    strSubfieldName,
                    strSubfieldValue);
        }

        // 2011/8/9
        /// <summary>
        /// 获得字一个子字段内容
        /// </summary>
        /// <param name="strFieldName">字段名</param>
        /// <param name="strSubfieldName">子字段名</param>
        /// <param name="strIndicatorMatch">字段指示符匹配条件。星号代表任意字符。如果第一字符为 '@'，表示使用正则表达式</param>
        /// <returns>子字段正文内容</returns>
        public string GetFirstSubfield(string strFieldName,
            string strSubfieldName,
            string strIndicatorMatch)
        {
            return this.DetailForm.MarcEditor.Record.Fields.GetFirstSubfield(
                    strFieldName,
                    strSubfieldName,
                    strIndicatorMatch);
        }

        // 2011/8/10
        /// <summary>
        /// 获得若干子字段内容
        /// </summary>
        /// <param name="strFieldName">字段名</param>
        /// <param name="strSubfieldName">子字段名</param>
        /// <param name="strIndicatorMatch">字段指示符匹配条件。星号代表任意字符。如果第一字符为 '@'，表示使用正则表达式</param>
        /// <returns>字符串集合</returns>
        public List<string> GetSubfields(string strFieldName,
            string strSubfieldName,
            string strIndicatorMatch = "**")
        {
            return this.DetailForm.MarcEditor.Record.Fields.GetSubfields(
                    strFieldName,
                    strSubfieldName,
                    strIndicatorMatch);
        }

        void DoGcatStop(object sender, StopEventArgs e)
        {
            if (this.GcatChannel != null)
                this.GcatChannel.Abort();
        }

        bool bMarcEditorFocued = false;

        /// <summary>
        /// 开始进行 GCAT 通讯操作
        /// </summary>
        /// <param name="strMessage">要在状态行显示的提示信息</param>
        public void BeginGcatLoop(string strMessage)
        {
            bMarcEditorFocued = this.DetailForm.MarcEditor.Focused;
            this.DetailForm.EnableControls(false);

            Stop stop = this.DetailForm.Progress;

            stop.OnStop += new StopEventHandler(this.DoGcatStop);
            stop.Initial(strMessage);
            stop.BeginLoop();

            this.DetailForm.Update();
            Program.MainForm.Update();
        }

        /// <summary>
        /// 结束 GCAT 通讯操作
        /// </summary>
        public void EndGcatLoop()
        {
            Stop stop = this.DetailForm.Progress;
            stop.EndLoop();
            stop.OnStop -= new StopEventHandler(this.DoGcatStop);
            stop.Initial("");

            this.DetailForm.EnableControls(true);
            if (bMarcEditorFocued == true)
                this.DetailForm.MarcEditor.Focus();
        }

        // 2012/4/1
        // 过滤出纯粹英文的字符串
        /// <summary>
        /// 过滤出纯粹英文的字符串
        /// </summary>
        /// <param name="authors">源字符串集合</param>
        /// <returns>过滤后的字符串集合</returns>
        public static List<string> NotContainHanzi(List<string> authors)
        {
            List<string> results = new List<string>();
            foreach (string strAuthor in authors)
            {
                if (ContainHanzi(strAuthor) == false)
                    results.Add(strAuthor);
            }

            return results;
        }

        // 过滤出包含汉字的字符串
        /// <summary>
        /// 过滤出包含汉字的字符串
        /// </summary>
        /// <param name="authors">源字符串集合</param>
        /// <returns>过滤后的字符串集合</returns>
        public static List<string> ContainHanzi(List<string> authors)
        {
            List<string> results = new List<string>();
            foreach (string strAuthor in authors)
            {
                if (ContainHanzi(strAuthor) == true)
                    results.Add(strAuthor);
            }

            return results;
        }

        /// <summary>
        /// 判断一个字符串内是否包含汉字
        /// </summary>
        /// <param name="strAuthor">要判断的字符串</param>
        /// <returns>是否包含汉字</returns>
        public static bool ContainHanzi(string strAuthor)
        {
            strAuthor = strAuthor.Trim();
            if (string.IsNullOrEmpty(strAuthor) == true)
                return false;

            string strError = "";
            string strResult = "";
            int nRet = PrepareSjhmAuthorString(strAuthor,
                out strResult,
                out strError);
            if (string.IsNullOrEmpty(strResult) == true)
                return false;
            return true;
        }

        // 对即将取四角号码的著者字符串进行预加工。例如去掉所有非汉字字符
        /// <summary>
        /// 对即将取四角号码的著者字符串进行预加工。例如去掉所有非汉字字符
        /// </summary>
        /// <param name="strAuthor">源字符串</param>
        /// <param name="strResult">返回结果字符串</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 正常</returns>
        public static int PrepareSjhmAuthorString(string strAuthor,
            out string strResult,
            out string strError)
        {
            strResult = "";
            strError = "";

            // string strSpecialChars = "！·＃￥％……—＊（）——＋－＝［］《》＜＞，。？／＼｜｛｝“”‘’";

            for (int i = 0; i < strAuthor.Length; i++)
            {
                char ch = strAuthor[i];

                if (StringUtil.IsHanzi(ch) == false)
                    continue;

                // 看看是否特殊符号
                if (StringUtil.SpecialChars.IndexOf(ch) != -1)
                {
                    continue;
                }

                // 汉字
                strResult += ch;
            }

            return 0;
        }

        // 2011/12/18
        // 获得著者号 -- Cutter-Sanborn Three-Figure
        // return:
        //      -1  error
        //      0   canceled
        //      1   succeed
        /// <summary>
        /// 获得卡特 (Cutter-Sanborn Three-Figure) 著者号。本函数可以被脚本重载
        /// </summary>
        /// <param name="strAuthor">著者字符串</param>
        /// <param name="strAuthorNumber">返回著者号</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 放弃; 1: 成功</returns>
        public virtual int GetCutterAuthorNumber(string strAuthor,
            out string strAuthorNumber,
            out string strError)
        {
            strError = "";
            strAuthorNumber = "";

            int nRet = Program.MainForm.LoadQuickCutter(true, out strError);
            if (nRet == -1)
                return -1;

            string strText = "";
            string strNumber = "";
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            nRet = Program.MainForm.QuickCutter.GetEntry(strAuthor,
                out strText,
                out strNumber,
                out strError);
            if (nRet == -1 || nRet == 0)
                return -1;

            strAuthorNumber = strText[0] + strNumber;
            return 1;
        }

        // 获得著者号 -- 四角号码
        // return:
        //      -1  error
        //      0   canceled
        //      1   succeed
        /// <summary>
        /// 获得字叫号码著者号。本函数可以被脚本重载
        /// </summary>
        /// <param name="strAuthor">著者字符串</param>
        /// <param name="strAuthorNumber">返回著者号</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 放弃; 1: 成功</returns>
        public virtual int GetSjhmAuthorNumber(string strAuthor,
            out string strAuthorNumber,
            out string strError)
        {
            strError = "";
            strAuthorNumber = "";

            string strResult = "";
            int nRet = PrepareSjhmAuthorString(strAuthor,
            out strResult,
            out strError);
            if (nRet == -1)
                return -1;
            if (String.IsNullOrEmpty(strResult) == true)
            {
                strError = "著者字符串 '" + strAuthor + "' 里面没有包含有效的汉字字符";
                return -1;
            }

            List<string> sjhms = null;
            // 把字符串中的汉字转换为四角号码
            // parameters:
            //      bLocal  是否从本地获取四角号码
            // return:
            //      -1  出错
            //      0   用户希望中断
            //      1   正常
            nRet = Program.MainForm.HanziTextToSjhm(
                true,
                strResult,
                out sjhms,
                out strError);
            if (nRet != 1)
                return nRet;

            if (strResult.Length != sjhms.Count)
            {
                strError = "著者字符串 '" + strResult + "' 里面的字符数(" + strResult.Length.ToString() + ")和取四角号码后的结果事项个数 " + sjhms.Count.ToString() + " 不符";
                return -1;
            }

            // 1，著者名称为一字者，取该字的四角号码。如：肖=9022
            if (strResult.Length == 1)
            {
                strAuthorNumber = sjhms[0].Substring(0, 4);
                return 1;
            }
            // 2，著者名称为二字者，分别取两个字的左上角和右上角。如：刘翔=0287
            if (strResult.Length == 2)
            {
                strAuthorNumber = sjhms[0].Substring(0, 2) + sjhms[1].Substring(0, 2);
                return 1;
            }

            // 3，著者名称为三字者，依次取首字左上、右上两角和后两字的左上角。如：罗贯中=6075
            if (strResult.Length == 3)
            {
                strAuthorNumber = sjhms[0].Substring(0, 2)
                    + sjhms[1].Substring(0, 1)
                    + sjhms[2].Substring(0, 1);
                return 1;
            }

            // 4，著者名称为四字者，依次取各字的左上角。如：中田英寿=5645
            // 5，五字及以上字数者，均以前四字取号，方法同上。如：奥斯特洛夫斯基=2423
            if (strResult.Length >= 4)
            {
                strAuthorNumber = sjhms[0].Substring(0, 1)
                    + sjhms[1].Substring(0, 1)
                    + sjhms[2].Substring(0, 1)
                    + sjhms[3].Substring(0, 1);
                return 1;
            }

            strError = "error end";
            return -1;
        }

        // 获得著者号
        // return:
        //      -4  著者字符串没有检索命中
        //      -1  error
        //      0   canceled
        //      1   succeed
        /// <summary>
        /// 获得 GCAT (通用汉语著者号码表) 著者号
        /// </summary>
        /// <param name="strGcatWebServiceUrl">GCAT Webservice URL 地址</param>
        /// <param name="strAuthor">著者字符串</param>
        /// <param name="strAuthorNumber">返回著者号</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 放弃; 1: 成功</returns>
        public int GetGcatAuthorNumber(string strGcatWebServiceUrl,
            string strAuthor,
            out string strAuthorNumber,
            out string strError)
        {
            strError = "";
            strAuthorNumber = "";

            if (String.IsNullOrEmpty(strGcatWebServiceUrl) == true)
                strGcatWebServiceUrl = "http://dp2003.com/dp2library/";  // "http://dp2003.com/gcatserver/"    //  "http://dp2003.com/dp2libraryws/gcat.asmx";

            if (strGcatWebServiceUrl.IndexOf(".asmx") != -1)
            {

                if (this.GcatChannel == null)
                    this.GcatChannel = new DigitalPlatform.GcatClient.Channel();

                string strDebugInfo = "";

                BeginGcatLoop("正在获取 '" + strAuthor + "' 的著者号，从 " + strGcatWebServiceUrl + " ...");
                try
                {
                    // return:
                    //      -1  error
                    //      0   canceled
                    //      1   succeed
                    int nRet = this.GcatChannel.GetNumber(
                        this.DetailForm.Progress,
                        this.DetailForm,
                        strGcatWebServiceUrl,
                        strAuthor,
                        true,	// bSelectPinyin
                        true,	// bSelectEntry
                        true,	// bOutputDebugInfo
                        new DigitalPlatform.GcatClient.BeforeLoginEventHandle(gcat_channel_BeforeLogin),
                        out strAuthorNumber,
                        out strDebugInfo,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "取 著者 '" + strAuthor + "' 之号码时出错 : " + strError;
                        return -1;
                    }

                    return nRet;
                }
                finally
                {
                    EndGcatLoop();
                }
            }
            else if (strGcatWebServiceUrl.Contains("gcat"))
            {
                // 新的WebService

                string strID = Program.MainForm.AppInfo.GetString("DetailHost", "gcat_id", "");
                bool bSaveID = Program.MainForm.AppInfo.GetBoolean("DetailHost", "gcat_saveid", false);

                Hashtable question_table = (Hashtable)Program.MainForm.ParamTable["question_table"];
                if (question_table == null)
                    question_table = new Hashtable();

                REDO_GETNUMBER:
                string strDebugInfo = "";

                BeginGcatLoop("正在获取 '" + strAuthor + "' 的著者号，从 " + strGcatWebServiceUrl + " ...");
                try
                {
                    // return:
                    //      -1  error
                    //      0   canceled
                    //      1   succeed
                    int nRet = GcatNew.GetNumber(
                        ref question_table,
                        this.DetailForm.Progress,
                        this.DetailForm,
                        strGcatWebServiceUrl,
                        strID, // ID
                        strAuthor,
                        true,	// bSelectPinyin
                        true,	// bSelectEntry
                        true,	// bOutputDebugInfo
                        out strAuthorNumber,
                        out strDebugInfo,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "取 著者 '" + strAuthor + "' 之号码时出错 : " + strError;
                        return -1;
                    }
                    if (nRet == -2)
                    {
                        IdLoginDialog login_dlg = new IdLoginDialog();
                        GuiUtil.SetControlFont(login_dlg, Program.MainForm.DefaultFont, false);
                        login_dlg.Text = "获得著者号 -- "
                            + ((string.IsNullOrEmpty(strID) == true) ? "请输入ID" : strError);
                        login_dlg.ID = strID;
                        login_dlg.SaveID = bSaveID;
                        login_dlg.StartPosition = FormStartPosition.CenterScreen;
                        if (login_dlg.ShowDialog(this.DetailForm) == DialogResult.Cancel)
                        {
                            return -1;
                        }

                        strID = login_dlg.ID;
                        bSaveID = login_dlg.SaveID;
                        if (login_dlg.SaveID == true)
                        {
                            Program.MainForm.AppInfo.SetString("DetailHost", "gcat_id", strID);
                        }
                        else
                        {
                            Program.MainForm.AppInfo.SetString("DetailHost", "gcat_id", "");
                        }
                        Program.MainForm.AppInfo.SetBoolean("DetailHost", "gcat_saveid", bSaveID);
                        goto REDO_GETNUMBER;
                    }

                    Program.MainForm.ParamTable["question_table"] = question_table;

                    return nRet;
                }
                finally
                {
                    EndGcatLoop();
                }
            }
            else // dp2library 服务器
            {
                Hashtable question_table = (Hashtable)Program.MainForm.ParamTable["question_table"];
                if (question_table == null)
                    question_table = new Hashtable();

                string strDebugInfo = "";

                BeginGcatLoop("正在获取 '" + strAuthor + "' 的著者号，从 " + strGcatWebServiceUrl + " ...");
                try
                {
                    // return:
                    //      -4  著者字符串没有检索命中
                    //      -2  strID验证失败
                    //      -1  error
                    //      0   canceled
                    //      1   succeed
                    long nRet = BiblioItemsHost.GetAuthorNumber(
                        ref question_table,
                        this.DetailForm.Progress,
                        this.DetailForm,
                        strGcatWebServiceUrl,
                        strAuthor,
                        true,	// bSelectPinyin
                        true,	// bSelectEntry
                        true,	// bOutputDebugInfo
                        out strAuthorNumber,
                        out strDebugInfo,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "取 著者 '" + strAuthor + "' 之号码时出错 : " + strError;
                        return -1;
                    }
                    Program.MainForm.ParamTable["question_table"] = question_table;
                    return (int)nRet;
                }
                finally
                {
                    EndGcatLoop();
                }
            }
        }

        // 从一个索取号字符串中析出第一行
        // parameters:
        //      strCallNumberStyle  索取号形态。为 索取类号+区分号/馆藏代码+索取类号+区分号 之一。缺省为前者
        /// <summary>
        /// 从一个索取号字符串中析出 馆藏代码 行
        /// </summary>
        /// <param name="strCallNumberStyle">索取号形态。为 "索取类号+区分号" "馆藏代码+索取类号+区分号" 之一。缺省为前者</param>
        /// <param name="strCallNumber">索取号字符串</param>
        /// <returns>馆藏代码 行。如果索取号中没有这一行，就返回 null</returns>
        public virtual string GetHeadLinePart(
            string strCallNumberStyle,
            string strCallNumber)
        {
#if NO
            string[] parts = strCallNumber.Split(new char[] { '/' });

            if (string.IsNullOrEmpty(strCallNumberStyle) == true
                || strCallNumberStyle == "两行"
                || strCallNumberStyle == "二行")
            {
                return null;
            }
            else if (strCallNumberStyle == "馆藏代码+索取类号+区分号"
                || strCallNumberStyle == "三行")
            {
                if (parts.Length > 0)
                    return parts[0].Trim();
            }
            return null;
#endif
            // 这种取法容错性更好
            return StringUtil.GetCallNumberHeadLine(strCallNumber);
        }

        // 从一个索取号字符串中析出类号部分
        // 当索取号有HeadLine时，需要重载本函数，取第二行为索取类号。注：有了排架信息后，就不必重载本函数了
        // parameters:
        //      strCallNumberStyle  索取号形态。为 索取类号+区分号/馆藏代码+索取类号+区分号 之一。缺省为前者
        /// <summary>
        /// 从一个索取号字符串中析出类号部分
        /// </summary>
        /// <param name="strCallNumberStyle">索取号形态。为 "索取类号+区分号" "馆藏代码+索取类号+区分号" 之一。缺省为前者</param>
        /// <param name="strCallNumber">索取号字符串</param>
        /// <returns>索取类号 行</returns>
        public virtual string GetClassPart(
            string strCallNumberStyle,
            string strCallNumber)
        {
#if NO
            string[] parts = strCallNumber.Split(new char[] {'/'});

            if (string.IsNullOrEmpty(strCallNumberStyle) == true
    || strCallNumberStyle == "索取类号+区分号"
    || strCallNumberStyle == "两行"
                || strCallNumberStyle == "二行")
            {
                if (parts.Length > 0)
                    return parts[0].Trim();
            }
            else if (strCallNumberStyle == "馆藏代码+索取类号+区分号"
                || strCallNumberStyle == "三行")
            {
                if (parts.Length > 1)
                    return parts[1].Trim();
            }
#endif
            // 这种取法容错性更好
            strCallNumber = StringUtil.BuildLocationClassEntry(strCallNumber);

            string[] parts = strCallNumber.Split(new char[] { '/' });
            if (parts.Length > 0)
                return parts[0].Trim();

            return "";
        }

        // (从MARC编辑器中)获得索取号类号部分
        // 重载说明：本函数管辖从strClassType映射到索取号类号部分。
        //      本函数下面还调用了GetCallNumberClassSource()
        //      如果重载了本函数，就有机会改变strClassType和来源字段名/子字段名的映射逻辑，以及从MARC编辑器内获取索取类号字符串的逻辑
        // return:
        //      -1  error
        //      0   MARC记录中没有找到所需的来源字段/子字段内容
        //      1   succeed
        /// <summary>
        /// 从MARC编辑器中获得索取号类号部分
        /// 重载说明：本函数管辖从 strClassType 映射到索取号类号部分。
        ///      本函数内部调用了 GetCallNumberClassSource() 实现功能
        ///      如果重载了本函数，就有机会改变 strClassType 和来源字段名/子字段名的映射逻辑，以及从 MARC 编辑器内获取索取类号字符串的逻辑
        /// </summary>
        /// <param name="strClassType">类号类型</param>
        /// <param name="strClass">返回类号</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: MARC 记录中没有找到所需的来源字段/子字段内容; 1: 成功</returns>
        public virtual int GetCallNumberClassPart(
            string strClassType,
            out string strClass,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            strClass = "";

            string strFieldName = "";
            string strSubfieldName = "";

            // 获得索取号类号部分的来源字段名和子字段名
            // return:
            //      -1  error
            //      1   succeed
            nRet = GetCallNumberClassSource(
                strClassType,
                out strFieldName,
                out strSubfieldName,
                out strError);
            if (nRet == -1)
            {
                strError = "获取索取号类号部分来源字段和子字段名时出错: " + strError;
                return -1;
            }

            strClass = this.DetailForm.MarcEditor.Record.Fields.GetFirstSubfield(
                strFieldName,
                strSubfieldName);
            if (String.IsNullOrEmpty(strClass) == true)
            {
                strError = "MARC记录中 " + strFieldName + "$" + strSubfieldName + " 没有找到，因此无法获得索取类号";
                return 0;
            }

            return 1;
        }

        // 获得索取号类号部分的来源字段名和子字段名
        // 重载说明：本函数管辖从strClassType映射到索取号类号部分的来源字段名/子字段名。
        //      如果仅重载本函数，并不会改变(上层函数GetCallNumberClassPart())从MARC编辑器内获取索取类号字符串的逻辑
        // return:
        //      -1  error
        //      1   succeed
        /// <summary>
        /// 获得索取号类号部分的来源字段名和子字段名
        /// 重载说明：本函数管辖从 strClassType 映射到索取号类号部分的来源字段名/子字段名。
        ///      如果仅重载本函数，并不会改变(上层函数GetCallNumberClassPart())从 MARC 编辑器内获取索取类号字符串的逻辑
        /// </summary>
        /// <param name="strClassType">类号类型</param>
        /// <param name="strFieldName">返回字段名</param>
        /// <param name="strSubfieldName">返回子字段名</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public virtual int GetCallNumberClassSource(
            string strClassType,
            out string strFieldName,
            out string strSubfieldName,
            out string strError)
        {
            strError = "";
            strFieldName = "";
            strSubfieldName = "";

            string strMarcSyntax = this.DetailForm.GetCurrentMarcSyntax();

            if (strMarcSyntax == "unimarc")
            {
                if (strClassType == "中图法")
                {
                    strFieldName = "690";
                    strSubfieldName = "a";
                }
                else if (strClassType == "科图法")
                {
                    strFieldName = "692";
                    strSubfieldName = "a";
                }
                else if (strClassType == "人大法")
                {
                    strFieldName = "694";
                    strSubfieldName = "a";
                }
                else if (strClassType == "其它" || strClassType == "红泥巴")
                {
                    strFieldName = "686";
                    strSubfieldName = "a";
                }
#if SHITOUTANG
                else if (strClassType == "石头汤分类法"
                    || strClassType == "石头汤分类号"
                    || strClassType == "石头汤")
                {
                    strFieldName = "687";
                    strSubfieldName = "a";
                }
#endif
                else
                {
                    strError = "UNIMARC下未知的分类法 '" + strClassType + "'";
                    return -1;
                }
            }
            else if (strMarcSyntax == "usmarc")
            {
                if (strClassType == "杜威十进分类号"
                    || strClassType == "杜威十进分类法"
                    || strClassType == "DDC")
                {
                    strFieldName = "082";
                    strSubfieldName = "a";
                }
                else if (strClassType == "国际十进分类号"
                    || strClassType == "国际十进分类法"
                    || strClassType == "UDC")
                {
                    strFieldName = "080";
                    strSubfieldName = "a";
                }
                else if (strClassType == "国会图书馆分类法"
                    || strClassType == "美国国会图书馆分类法"
                    || strClassType == "LCC")
                {
                    strFieldName = "050";
                    strSubfieldName = "a";
                }
                else if (strClassType == "中图法")
                {
                    strFieldName = "093";
                    strSubfieldName = "a";
                }
                else if (strClassType == "科图法")
                {
                    strFieldName = "094";
                    strSubfieldName = "a";
                }
                else if (strClassType == "人大法")
                {
                    strFieldName = "095";
                    strSubfieldName = "a";
                }
                else if (strClassType == "其它" || strClassType == "红泥巴")
                {
                    strFieldName = "084";
                    strSubfieldName = "a";
                }
#if SHITOUTANG
                else if (strClassType == "石头汤分类法"
                    || strClassType == "石头汤分类号"
                    || strClassType == "石头汤")
                {
                    strFieldName = "087";
                    strSubfieldName = "a";
                }
#endif
                else
                {
                    strError = "USMARC下未知的分类法 '" + strClassType + "'";
                    return -1;
                }
            }
            else
            {
                strError = "未知的MARC格式 '" + strMarcSyntax + "'";
                return -1;
            }

            return 1;
        }

        // 管理索取号
        // sender为EntityControl类型时，所选择的行只能为1行
        /// <summary>
        /// 管理索取号
        /// </summary>
        /// <param name="sender">事件触发者。当 sender 为 EntityControl 类型时，所选择的行只能为 1 行</param>
        /// <param name="e">事件参数</param>
        public virtual void ManageCallNumber(object sender,
            GenerateDataEventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (sender == null)
            {
                strError = "sender为null";
                goto ERROR1;
            }

            string strLocation = "";
            string strClass = "";
            string strItemRecPath = "";

            List<CallNumberItem> callnumber_items = null;

            ArrangementInfo info = null;

            if (sender is EntityEditForm)
            {
                EntityEditForm edit = (EntityEditForm)sender;

                // 取得馆藏地点
                strLocation = edit.entityEditControl_editing.LocationString;
                strLocation = StringUtil.GetPureLocationString(strLocation);  // 2009/3/29 

                /*
                if (String.IsNullOrEmpty(strLocation) == true)
                {
                    strError = "请先输入馆藏地点。否则无法管理索取号";
                    goto ERROR1;
                }*/


                // 获得关于一个特定馆藏地点的索取号配置信息
                nRet = Program.MainForm.GetArrangementInfo(strLocation,
                    out info,
                    out strError);
                if (nRet == 0)
                {
                    strError = "没有关于馆藏地点 '" + strLocation + "' 的排架体系配置信息，无法管理索取号";
                    goto ERROR1;
                }
                if (nRet == -1)
                    goto ERROR1;


                // 获得已有的类号
                strClass = GetClassPart(
                    info.CallNumberStyle,
                    edit.entityEditControl_editing.AccessNo);

                strItemRecPath = edit.entityEditControl_editing.RecPath;

#if REF
                // 2017/4/6
                if (string.IsNullOrEmpty(strItemRecPath))
                {
                    strItemRecPath = "@refID:" + edit.entityEditControl_editing.RefID;
                    // TODO: 不应为空
                }
#endif

                callnumber_items = edit.Items.GetCallNumberItems();
            }
            else if (sender is EntityControl)
            {
                EntityControl control = (EntityControl)sender;

                if (control.ListView.SelectedIndices.Count == 0)
                {
                    strError = "请先选定要关注的行。否则无法管理索取号";
                    goto ERROR1;
                }

                if (control.ListView.SelectedIndices.Count > 1)
                {
                    strError = "当前选定行多于1行(为 " + control.ListView.SelectedIndices.Count + " 行)。无法管理多个索取号。请只选择一行，然后再使用本功能";
                    goto ERROR1;
                }

                BookItem book_item = control.GetVisibleItemAt(control.ListView.SelectedIndices[0]);
                Debug.Assert(book_item != null, "");

                strLocation = book_item.Location;
                strLocation = StringUtil.GetPureLocationString(strLocation);  // 2009/3/29 

                /*
                if (String.IsNullOrEmpty(strLocation) == true)
                {
                    strError = "请先输入馆藏地点。否则无法创建索取号";
                    goto ERROR1;
                }*/

                // 获得关于一个特定馆藏地点的索取号配置信息
                nRet = Program.MainForm.GetArrangementInfo(strLocation,
                    out info,
                    out strError);
                if (nRet == 0)
                {
                    strError = "没有关于馆藏地点 '" + strLocation + "' 的排架体系配置信息，无法管理索取号";
                    goto ERROR1;
                }
                if (nRet == -1)
                    goto ERROR1;

                // 获得已有的类号
                strClass = GetClassPart(
                    info.CallNumberStyle,
                    book_item.AccessNo);

                strItemRecPath = book_item.RecPath;
#if REF
                // 2017/4/6
                if (string.IsNullOrEmpty(strItemRecPath))
                {
                    strItemRecPath = "@refID:" + book_item.RefID;
                    // TODO: 不应为空
                }
#endif
                callnumber_items = control.Items.GetCallNumberItems();
            }
            else if (sender is BindingForm)
            {
                BindingForm binding = (BindingForm)sender;

                // 取得馆藏地点
                strLocation = binding.EntityEditControl.LocationString;
                strLocation = StringUtil.GetPureLocationString(strLocation);  // 2009/3/29 

                // 获得关于一个特定馆藏地点的索取号配置信息
                nRet = Program.MainForm.GetArrangementInfo(strLocation,
                    out info,
                    out strError);
                if (nRet == 0)
                {
                    strError = "没有关于馆藏地点 '" + strLocation + "' 的排架体系配置信息，无法管理索取号";
                    goto ERROR1;
                }
                if (nRet == -1)
                    goto ERROR1;


                // 获得已有的类号
                strClass = GetClassPart(
                    info.CallNumberStyle,
                    binding.EntityEditControl.AccessNo);

                strItemRecPath = binding.EntityEditControl.RecPath;

#if REF
                // 2017/4/6
                if (string.IsNullOrEmpty(strItemRecPath))
                {
                    strItemRecPath = "@refID:" + binding.EntityEditControl.RefID;
                    // TODO: 不应为空
                }
#endif
                callnumber_items = binding.GetCallNumberItems();    // ???
            }
            else
            {
                strError = "sender必须是EntityEditForm或EntityControl或BindingForm类型(当前为" + sender.GetType().ToString() + ")";
                goto ERROR1;
            }


            // 如果当前册记录中不存在索取号类号，则去MARC记录中找
            if (String.IsNullOrEmpty(strClass) == true)
            {
                // (从MARC编辑器中)获得索取号类号部分
                // return:
                //      -1  error
                //      0   MARC记录中没有找到所需的来源字段/子字段内容
                //      1   succeed
                nRet = GetCallNumberClassPart(
                    info.ClassType,
                    out strClass,
                    out strError);
                if (nRet == -1)
                {
                    strError = "获得索取类号时出错: " + strError;
                    goto ERROR1;
                }
                if (nRet == 0)
                {
                    goto ERROR1;
                }

                Debug.Assert(nRet == 1, "");

                MessageBox.Show(this.DetailForm, "因当前册记录中没有索取号，自动从书目记录中取得索取类号 '" + strClass + "'，用以启动索取号管理");
            }

            CallNumberForm dlg = new CallNumberForm();

            // dlg.MdiParent = Program.MainForm;   // 打开为MDI子窗口
            dlg.MainForm = Program.MainForm;
            // dlg.TopMost = true;  // 打开为无模式对话框
            dlg.MyselfItemRecPath = strItemRecPath;
            dlg.MyselfParentRecPath = this.DetailForm.BiblioRecPath;

            dlg.MyselfCallNumberItems = callnumber_items;   // 2009/6/4 

            dlg.ClassNumber = strClass;
            dlg.LocationString = strLocation;
            dlg.AutoBeginSearch = true;

            dlg.Floating = true;

            dlg.FormClosed -= new FormClosedEventHandler(dlg_FormClosed);
            dlg.FormClosed += new FormClosedEventHandler(dlg_FormClosed);

            Program.MainForm.AppInfo.LinkFormState(dlg, "callnumber_floating_state");
            dlg.Show();

            return;
            ERROR1:
            MessageBox.Show(this.DetailForm, strError);
        }

        void dlg_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (sender != null)
            {
                // Program.MainForm.AppInfo.UnlinkFormState(sender as Form);
            }
        }
#if NO
        #region 乐山图书馆四角号码。这里是验证用。实际应用的时候需要写在脚本中

        // 获得种次号以外的其他区分号，主要是著者号
        // return:
        //      -1  error
        //      0   not found。注意此时也要设置strError值
        //      1   found
        public int LstsgGetAuthorNumber(string strQufenhaoType,
                out string strQufenhao,
                out string strError)
        {
            strError = "";
            strQufenhao = "";

            if (strQufenhaoType == "四角号码")
            {
                bool bPerson = false;

                List<string> results = null;
                // 700、710、720
                results = GetSubfields("700", "a", "@[^A].");    // 指示符
                results = ContainHanzi(results);
                if (results.Count > 0)
                {
                    bPerson = true;
                    goto FOUND;
                }
                results = GetSubfields("710", "a");
                results = ContainHanzi(results);
                if (results.Count > 0)
                {
                    bPerson = false;
                    goto FOUND;
                }
                results = GetSubfields("720", "a");
                results = ContainHanzi(results);
                if (results.Count > 0)
                {
                    bPerson = false;
                    goto FOUND;
                }

                // 701/711/702/712
                results = GetSubfields("701", "a", "@[^A].");   // 指示符
                results = ContainHanzi(results);
                if (results.Count > 0)
                {
                    bPerson = true;
                    goto FOUND;
                }

                results = GetSubfields("711", "a");
                results = ContainHanzi(results);
                if (results.Count > 0)
                {
                    bPerson = false;
                    goto FOUND;
                }

                results = GetSubfields("702", "a", "@[^A].");   // 指示符
                results = ContainHanzi(results);
                if (results.Count > 0)
                {
                    bPerson = true;
                    goto FOUND;
                }

                results = GetSubfields("712", "a");
                results = ContainHanzi(results);
                if (results.Count > 0)
                {
                    bPerson = false;
                    goto FOUND;
                }

                strError = "MARC记录中 700/710/720/701/711/702/712中均未发现包含汉字的 $a 子字段内容，无法获得著者字符串";
                return -1;
            FOUND:
                Debug.Assert(results.Count > 0, "");
                if (bPerson == true)
                {
                    // 获得著者号
                    // return:
                    //      -1  error
                    //      0   canceled
                    //      1   succeed
                    int nRet = LstsgGetSjhmAuthorNumber(
                        results[0],
                        out strQufenhao,
                        out strError);
                    if (nRet != 1)
                        return nRet;
                    return 1;
                }

                string strISBN = GetFirstSubfield("010", "a");
                // 团体责任者，记录中没有ISBN
                if (String.IsNullOrEmpty(strISBN) == true)
                {
                    // 获得著者号
                    // return:
                    //      -1  error
                    //      0   canceled
                    //      1   succeed
                    int nRet = LstsgGetSjhmAuthorNumber(
                        results[0],
                        out strQufenhao,
                        out strError);
                    if (nRet != 1)
                        return nRet;
                    return 1;
                }

                // 团体责任者，记录中有ISBN
                if (strISBN.IndexOf("-") == -1)
                {
                    strError = "团体责任者取著者号的时候需要用到ISBN，但是当前ISBN '"+strISBN+"' 中没有符号'-'。请先为ISBN加上'-'以后再取著者号";
                    return -1;
                }
                try
                {
                    string strPublisher = IsbnSplitter.GetPublisherCode(strISBN);

                    if (string.IsNullOrEmpty(strPublisher) == true)
                    {
                        strError = "ISBN '"+strISBN+"' 中的竖版社代码部分格式不正确";
                        return -1;
                    }
                    // 出版社代号为三个数的，在三个数后面加零，出版社代号为五个数的，取前面四位数
                    if (strPublisher.Length < 4)
                        strQufenhao = strPublisher.PadRight(4-strPublisher.Length, '0');
                    else if (strPublisher.Length >= 5)
                        strQufenhao = strPublisher.Substring(0, 4);
                    else
                        strQufenhao = strPublisher;
                }
                catch (Exception ex)
                {
                    strError = ex.Message;
                    return -1;
                }

                return 1;
            }
            else
            {
                strError = "LstsgGetAuthorNumber() 只支持四角号码类型的著者号";
                return -1;
            }

        }


        // 获得著者号 -- 四角号码
        // return:
        //      -1  error
        //      0   canceled
        //      1   succeed
        public int LstsgGetSjhmAuthorNumber(string strAuthor,
            out string strAuthorNumber,
            out string strError)
        {
            strError = "";
            strAuthorNumber = "";

            string strResult = "";
            int nRet = PrepareSjhmAuthorString(strAuthor,
            out strResult,
            out strError);
            if (nRet == -1)
                return -1;
            if (String.IsNullOrEmpty(strResult) == true)
            {
                strError = "著者字符串 '" + strAuthor + "' 里面没有包含有效的汉字字符";
                return -1;
            }

            List<string> sjhms = null;
            // 把字符串中的汉字转换为四角号码
            // parameters:
            //      bLocal  是否从本地获取四角号码
            // return:
            //      -1  出错
            //      0   用户希望中断
            //      1   正常
            nRet = Program.MainForm.HanziTextToSjhm(
                true,
                strResult,
                out sjhms,
                out strError);
            if (nRet != 1)
                return nRet;

            if (strResult.Length != sjhms.Count)
            {
                strError = "著者字符串 '" + strResult + "' 里面的字符数(" + strResult.Length.ToString() + ")和取四角号码后的结果事项个数 " + sjhms.Count.ToString() + " 不符";
                return -1;
            }

            // 1，著者名称为一字者，为一字者，取左右上角，后面加两个零
            if (strResult.Length == 1)
            {
                strAuthorNumber = sjhms[0].Substring(0, 2) + "00";
                return 1;
            }
            // 2，著者名称为二字者，各取左右上角
            if (strResult.Length == 2)
            {
                strAuthorNumber = sjhms[0].Substring(0, 2) + sjhms[1].Substring(0, 2);
                return 1;
            }

            // 3，著者名称为三字者，第一个字取左右上角，后面二个字各取左上角
            if (strResult.Length == 3)
            {
                strAuthorNumber = sjhms[0].Substring(0, 2)
                    + sjhms[1].Substring(0, 1)
                    + sjhms[2].Substring(0, 1);
                return 1;
            }

            // 4，著者名称为四字者，取四个字的左上角
            if (strResult.Length >= 4)
            {
                strAuthorNumber = sjhms[0].Substring(0, 1)
                    + sjhms[1].Substring(0, 1)
                    + sjhms[2].Substring(0, 1)
                    + sjhms[3].Substring(0, 1);
                return 1;
            }

            // 5，五个字以上的只取前三个字
            if (strResult.Length >= 5)
            {
                strAuthorNumber = sjhms[0].Substring(0, 2)
                    + sjhms[1].Substring(0, 1)
                    + sjhms[2].Substring(0, 1);
                return 1;
            } 
            
            strError = "error end";
            return -1;
        }

        #endregion

#endif

        class AuthorLevel
        {
            /// <summary>
            /// 著者字符串
            /// </summary>
            public string Author = "";
            /// <summary>
            /// 著者字符串的级别 -1 出错信息, 0 没有找到著者的提示信息, 1 题名, 2 著者
            /// </summary>
            public float Level = 0;

            /// <summary>
            /// 区分号类型
            /// </summary>
            public string Type = "";
        }

        class AuthorLevelComparer : IComparer<AuthorLevel>
        {
            int IComparer<AuthorLevel>.Compare(AuthorLevel x, AuthorLevel y)
            {
                // 可以精确到 0.01
                return (int)(-100 * (x.Level - y.Level)); // 大在前
            }
        }


        // 注意：此函数和 BiblioItemsHost.cs 中的同名函数重复内容。请尽量保持同步修改
        // 获得种次号以外的其他区分号，主要是著者号
        // return:
        //      -1  error
        //      0   not found。注意此时也要设置strError值
        //      1   found
        /// <summary>
        /// 获得种次号以外的其他区分号，主要是著者号
        /// </summary>
        /// <param name="strQufenhaoTypes">区分号类型。可以是一个区分号类型，也可以是逗号间隔的若干个区分号类型</param>
        /// <param name="strQufenhao">返回区分号</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 没有找到(注意此时 strError 中也返回了内容); 1: 找到</returns>
        public virtual int GetAuthorNumber(string strQufenhaoTypes,
            out string strQufenhao,
            out string strError)
        {
            strError = "";
            strQufenhao = "";
            int nRet = 0;

            List<string> types = StringUtil.SplitList(strQufenhaoTypes);

            List<AuthorLevel> authors = new List<AuthorLevel>();

            // *** 第一阶段，遍历获取每个区分号类型的著者字符串
            foreach (string type in types)
            {
                string strAuthor = "";
                float fLevel = 0;

                AuthorLevel author = new AuthorLevel();
                author.Type = type;
                if (type == "GCAT"
                    || type == "四角号码"
                    || type == "Cutter-Sanborn Three-Figure"
#if SHITOUTANG

 || type == "石头汤著者号"
                    || type == "石头汤"
#endif
)
                {
                    // 根据区分号类型从MARC记录中获得作者字符串
                    // return:
                    //      -1  error
                    //      0   not found
                    //      1   found
                    nRet = GetAuthor(type,
                        out strAuthor,
                        out fLevel,
                        out strError);

#if DEBUG
                    if (nRet == 0)
                    {
                        Debug.Assert(String.IsNullOrEmpty(strAuthor) == true, "");
                    }

                    if (nRet == 1)
                    {
                        Debug.Assert(String.IsNullOrEmpty(strAuthor) == false, "");
                    }
#endif

                    if (nRet == -1 || nRet == 0)
                        author.Level = nRet;
                    else
                        author.Level = fLevel;
                    if (nRet == 1)
                        author.Author = strAuthor;
                    else
                        author.Author = strError;
                    authors.Add(author);
                    continue;
                }
                else if (type == "手动")
                {
                    author.Level = 1;
                    author.Author = "?";
                    authors.Add(author);
                    continue;
                }
                else if (type == "<无>")
                {
                    author.Level = 1;
                    author.Author = "";
                    authors.Add(author);
                    continue;
                }
                else
                {
                    strError = "未知的区分号类型 '" + type + "'";
                    goto ERROR1;
                }
            }

            // *** 第二阶段，选择一个 level 最高的著者信息
            AuthorLevel one = null;
            if (authors.Count == 0)
            {
                strError = "没有指定任何区分号类型，无法获得著者字符串";
                return 0;
            }
            else if (authors.Count == 1)
            {
                one = authors[0];
            }
            if (authors.Count > 1)
            {
                // TODO: 同 level 的排序，就需要一些暗示信息。例如记录来自某个数据库。UNIMARC 格式的库，可以让中文的字符串稍微级别高一点
                authors.Sort(new AuthorLevelComparer());

                one = authors[0];
                if (one.Level <= 0)
                {
                    string strWarning = "";
                    string strErrorText = "";
                    foreach (AuthorLevel author in authors)
                    {
                        if (author.Level == -1)
                        {
                            if (string.IsNullOrEmpty(strErrorText) == false)
                                strErrorText += "; ";
                            strErrorText += author.Author;
                        }
                        if (author.Level == 0)
                        {
                            if (string.IsNullOrEmpty(strWarning) == false)
                                strWarning += "; ";
                            strWarning += author.Author;
                        }
                    }

                    if (string.IsNullOrEmpty(strErrorText) == false)
                    {
                        strError = strErrorText;
                        return -1;
                    }

                    strError = strWarning;
                    return 0;
                }
            }

            if (one.Level == -1)
            {
                strError = one.Author;
                return -1;
            }
            if (one.Level == 0)
            {
                strError = one.Author;
                return 0;
            }

            // 2014/4/15
            if (one.Type == "<无>"
                || one.Type == "手动")
            {
                strQufenhao = one.Author;
                return 1;
            }

            // *** 第三阶段，从著者字符串创建著者号
            {
                string type = one.Type;
                string strAuthor = one.Author;
                Debug.Assert(String.IsNullOrEmpty(strAuthor) == false, "");
                REDO:

                if (type == "GCAT")
                {
                    string strPinyin = "";
                    List<string> two = StringUtil.ParseTwoPart(strAuthor, "|");
                    strAuthor = two[0];
                    strPinyin = two[1];

                    // 获得著者号
                    string strGcatWebServiceUrl = Program.MainForm.GcatServerUrl;   // "http://dp2003.com/dp2libraryws/gcat.asmx";

                    // 获得著者号
                    // return:
                    //      -4  著者字符串没有检索命中
                    //      -1  error
                    //      0   canceled
                    //      1   succeed
                    nRet = GetGcatAuthorNumber(strGcatWebServiceUrl,
                        strAuthor,
                        out strQufenhao,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 放弃回答问题
                    if (nRet == 0)
                    {
                        if (string.IsNullOrEmpty(strError) == true)
                            strError = "放弃从 GCAT 取号";
                        return 0;
                    }

                    if (nRet == -4)
                    {
                        string strHanzi = strAuthor;
                        string strLastError = strError;
                        // 临时取汉字的拼音
                        if (string.IsNullOrEmpty(strPinyin))
                        {
                            strPinyin = strAuthor;
                            // 取拼音
                            nRet = HanziToPinyin(ref strPinyin,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                        }
                        if (string.IsNullOrEmpty(strPinyin))
                            goto ERROR1;
                        string strMessage = "字符串 '" + strHanzi + "' 取汉语著者号码时出现意外状况: " + strLastError + "\r\n\r\n后面软件会自动尝试用卡特表方式为拼音字符串 '" + strPinyin + "' 取号。";
                        strAuthor = strPinyin;
                        type = "Cutter-Sanborn Three-Figure";
                        MessageBox.Show(this.DetailForm, strMessage);

                        // 尝试把信息发给 dp2003.com
                        Program.MainForm.ReportError("dp2circulation 创建索取号", "(安静汇报)" + strMessage);

                        goto REDO;
                    }

                    return 1;
                }
                else if (type == "四角号码")
                {
                    // 获得著者号
                    // return:
                    //      -1  error
                    //      0   canceled
                    //      1   succeed
                    nRet = GetSjhmAuthorNumber(
                        strAuthor,
                        out strQufenhao,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 放弃回答问题
                    if (nRet == 0)
                    {
                        if (string.IsNullOrEmpty(strError) == true)
                            strError = "放弃从四角号码取号";
                        return 0;
                    }
                    return 1;
                }
                else if (type == "Cutter-Sanborn Three-Figure")
                {
                    // 获得著者号
                    // return:
                    //      -1  error
                    //      0   canceled
                    //      1   succeed
                    nRet = GetCutterAuthorNumber(
                        strAuthor,
                        out strQufenhao,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 放弃回答问题
                    if (nRet == 0)
                    {
                        if (string.IsNullOrEmpty(strError) == true)
                            strError = "放弃从卡特表取号";
                        return 0;
                    }

                    return 1;
                }
#if SHITOUTANG
                else if (type == "石头汤著者号"
                    || type == "石头汤")
                {
                    strQufenhao = strAuthor;
                    return 1;
                }
#endif
                else if (type == "手动")
                {
                    strQufenhao = "?";
                    return 1;
                }
                else if (type == "<无>")
                {
                    strQufenhao = "";
                    return 1;
                }
                else
                {
                    strError = "未知的区分号类型 '" + type + "'";
                    goto ERROR1;
                }
            }
            //return 0;
            ERROR1:
            return -1;
        }

        // 根据区分号类型从MARC记录中获得作者字符串
        // return:
        //      -1  error
        //      0   not found。注意此时也要设置strError值
        //      1   found
        /// <summary>
        /// 根据区分号类型从 MARC 记录中获得著者字符串
        /// </summary>
        /// <param name="strQufenhaoType">区分号类型。注意这里是一个区分号类型</param>
        /// <param name="strAuthor">返回著者字符串</param>
        /// <param name="fLevel">返回所找到的著者字符串的级别。1 表示题名, 2表示著者。没有找到或者错误的情况，这里的值无效</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 没有找到(注意此时 strError 中也返回了内容); 1: 找到</returns>
        public virtual int GetAuthor(string strQufenhaoType,
            out string strAuthor,
            out float fLevel,
            out string strError)
        {
            strError = "";
            strAuthor = "";
            fLevel = 2;

            string strPinyin = "";

            string strMarcSyntax = this.DetailForm.GetCurrentMarcSyntax();

            if (strQufenhaoType == "GCAT")
            {
                if (strMarcSyntax == "unimarc")
                {

#if NO
                    List<string> results = null;
                    // 700、710、720
                    results = GetSubfields("700", "a", "@[^A].");    // 指示符
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }
                    results = GetSubfields("710", "a");
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }
                    results = GetSubfields("720", "a");
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    // 701/711/702/712
                    results = GetSubfields("701", "a", "@[^A].");   // 指示符
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    results = GetSubfields("711", "a");
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    results = GetSubfields("702", "a", "@[^A].");   // 指示符
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    results = GetSubfields("712", "a");
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    results = GetSubfields("200", "a");
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        fLevel = 1.1F;
                        goto FOUND;
                    }


                    strError = "MARC记录中 700/710/720/701/711/702/712/200 中均未发现包含汉字的 $a 子字段内容，无法获得著者字符串";
                    return 0;
                FOUND:
                    Debug.Assert(results.Count > 0, "");
                    strAuthor = results[0];
#endif
                    MarcRecord record = new MarcRecord(this.DetailForm.MarcEditor.Marc);

                    // 获得一个著者字符串和对应的拼音
                    // return:
                    //      -1  出错
                    //      0   没有找到
                    //      1   找到
                    int nRet = GetAuthorAndPinyin(record,
            out strAuthor,
            out strPinyin,
            out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)
                    {
                        strError = "MARC记录中 700/710/720/701/711/702/712/200 中均未发现包含汉字的 $a 子字段内容，无法获得著者字符串";
                        return 0;
                    }

                    if (string.IsNullOrEmpty(strPinyin) == false)
                        strAuthor += "|" + strPinyin;
                }
                else if (strMarcSyntax == "usmarc")
                {
                    List<string> locations = new List<string>();
                    locations.Add("100a");
                    locations.Add("110a");
                    locations.Add("111a");
                    locations.Add("700a");
                    locations.Add("710a");
                    locations.Add("711a");
                    strAuthor = GetFirstSubfield(locations);
                }
                else
                {
                    strError = "未知的MARC格式 '" + strMarcSyntax + "'";
                    return -1;
                }

                if (String.IsNullOrEmpty(strAuthor) == false)
                    return 1;   // found

                // TODO: 找到有汉字的
                // 最后找 245$a level 1.0F

                strError = "MARC记录中 100/110/111/700/710/711 中均未发现 $a， 无法获得著者字符串";
                fLevel = 0;
                return 0;   // not found
            }
            else if (strQufenhaoType == "四角号码")
            {
                if (strMarcSyntax == "unimarc")
                {
                    /*
                    List<string> locations = new List<string>();
                    locations.Add("701a");
                    locations.Add("711a");
                    locations.Add("702a");
                    locations.Add("712a");
                    strAuthor = GetFirstSubfield(locations);
                     * */
                    List<string> results = null;
                    // 700、710、720
                    results = GetSubfields("700", "a", "@[^A].");    // 指示符
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }
                    results = GetSubfields("710", "a");
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }
                    results = GetSubfields("720", "a");
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    // 701/711/702/712
                    results = GetSubfields("701", "a", "@[^A].");   // 指示符
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    results = GetSubfields("711", "a");
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    results = GetSubfields("702", "a", "@[^A].");   // 指示符
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    results = GetSubfields("712", "a");
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    results = GetSubfields("200", "a");
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        fLevel = 1.1F;
                        goto FOUND;
                    }

                    strError = "MARC记录中 700/710/720/701/711/702/712/200 中均未发现包含汉字的 $a 子字段内容，无法获得著者字符串";
                    fLevel = 0;
                    return 0;
                    FOUND:
                    Debug.Assert(results.Count > 0, "");
                    strAuthor = results[0];
                }
                else if (strMarcSyntax == "usmarc")
                {
                    List<string> locations = new List<string>();
                    locations.Add("100a");
                    locations.Add("110a");
                    locations.Add("111a");
                    locations.Add("700a");
                    locations.Add("710a");
                    locations.Add("711a");
                    strAuthor = GetFirstSubfield(locations);
                }
                else
                {
                    strError = "未知的 MARC格式 '" + strMarcSyntax + "'";
                    return -1;
                }

                if (String.IsNullOrEmpty(strAuthor) == false)
                    return 1;   // found

                strError = "MARC 记录中 100/110/111/700/710/711 中均未发现 $a 无法获得著者字符串";
                fLevel = 0;
                return 0;   // not found
            }
            else if (strQufenhaoType == "Cutter-Sanborn Three-Figure")
            {
                if (strMarcSyntax == "unimarc")
                {
                    List<string> results = null;
                    // 700、710、720
                    results = GetSubfields("700", "a", "@[^A].");    // 指示符
                    results = NotContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }
                    results = GetSubfields("710", "a");
                    results = NotContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }
                    results = GetSubfields("720", "a");
                    results = NotContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    // 701/711/702/712
                    results = GetSubfields("701", "a", "@[^A].");   // 指示符
                    results = NotContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    results = GetSubfields("711", "a");
                    results = NotContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    results = GetSubfields("702", "a", "@[^A].");   // 指示符
                    results = NotContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    results = GetSubfields("712", "a");
                    results = NotContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    results = GetSubfields("200", "a");
                    results = NotContainHanzi(results);
                    if (results.Count > 0)
                    {
                        fLevel = 1.0F;   // unimarc 格式中找到题名字符串，英文，作为卡特表用途，要弱一些
                        goto FOUND;
                    }

                    strError = "MARC记录中 700/710/720/701/711/702/712/200 中均未发现不含汉字的 $a 子字段内容，无法获得西文著者字符串";
                    fLevel = 0;
                    return 0;
                    FOUND:
                    Debug.Assert(results.Count > 0, "");
                    strAuthor = results[0];
                }
                else if (strMarcSyntax == "usmarc")
                {
                    List<string> locations = new List<string>();
                    locations.Add("100a");
                    locations.Add("110a");
                    locations.Add("111a");
                    locations.Add("700a");
                    locations.Add("710a");
                    locations.Add("711a");
                    strAuthor = GetFirstSubfield(locations);
                }
                else
                {
                    strError = "未知的MARC格式 '" + strMarcSyntax + "'";
                    return -1;
                }

                if (String.IsNullOrEmpty(strAuthor) == false)
                    return 1;   // found

                // TODO: 245$a 中找到的英文的题名字符串，要强一些，level 1.1

                strError = "MARC 记录中 100/110/111/700/710/711 中均未发现 $a， 无法获得著者字符串";
                fLevel = 0;
                return 0;   // not found
            }
#if SHITOUTANG
            else if (strQufenhaoType == "石头汤著者号"
                || strQufenhaoType == "石头汤")
            {
                MarcRecord record = new MarcRecord(this.DetailForm.MarcEditor.Marc);

                if (strMarcSyntax == "unimarc")
                {
                    MarcNodeList fields = record.select("field[@name='700' or @name='701' or @name='702' or @name='710' or @name='711' or @name='712']");

                    // 从选定的字段中获得石头汤著者字符串
                    // return:
                    //      -1  出错
                    //      0   没有找到
                    //      1   找到
                    int nRet = GetShitoutangAuthorString(fields,
                        out strAuthor,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 1)
                    {
                        fLevel = 2;
                        return 1;
                    }
#if NO
                    if (nRet == 0)
                    {
                        strError = "MARC记录中 700/710/720/701/711/702/712中均未发现著者字符串";
                        nLevel = 0;
                        return 0;

                    }
#endif

                    fields = record.select("field[@name='200']");

                    // 从选定的字段中获得石头汤著者字符串
                    // return:
                    //      -1  出错
                    //      0   没有找到
                    //      1   找到
                    nRet = GetShitoutangAuthorString(fields,
                        out strAuthor,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)
                    {
                        strError = "UNIMARC记录 700/710/720/701/711/702/712 字段均未发现著者字符串, 200字段中也未发现题名";
                        fLevel = 0;
                        return 0;
                    }
                    fLevel = 1;
                    return 1;
                }
                else if (strMarcSyntax == "usmarc")
                {
                    MarcNodeList fields = record.select("field[@name='100' or @name='110' or @name='111' or @name='700' or @name='710' or @name='711']");

                    // 从选定的字段中获得石头汤著者字符串
                    // return:
                    //      -1  出错
                    //      0   没有找到
                    //      1   找到
                    int nRet = GetShitoutangAuthorString(fields,
                        out strAuthor,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 1)
                    {
                        fLevel = 2;
                        return 1;
                    }
#if NO
                    if (nRet == 0)
                    {
                        strError = "MARC记录中 100/110/111/700/710/711 中均未发现著者字符串";
                        nLevel = 0;
                        return 0;
                    }
#endif


                    fields = record.select("field[@name='245']");

                    // 从选定的字段中获得石头汤著者字符串
                    // return:
                    //      -1  出错
                    //      0   没有找到
                    //      1   找到
                    nRet = GetShitoutangAuthorString(fields,
                        out strAuthor,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)
                    {
                        strError = "USMARC记录 700/710/720/701/711/702/712 字段均未发现著者字符串, 245字段中也未发现题名";
                        fLevel = 0;
                        return 0;
                    }
                    fLevel = 1;
                    return 1;
                }
                else
                {
                    strError = "未知的MARC格式 '" + strMarcSyntax + "'";
                    return -1;
                }
                fLevel = 0;
                return 0;
            }
#endif

            strError = "GetAuthor()时未知的区分号类型 '" + strQufenhaoType + "'";
            fLevel = -1;
            return -1;
        }

#if SHITOUTANG

        #region 石头汤著者号

        static string FirstContent(MarcNodeList nodes)
        {
            if (nodes.count == 0)
                return "";
            return nodes[0].Content;
        }

        // 把倒序的人名颠倒过来
        static string Reverse(string strText)
        {
            int nRet = strText.IndexOf(",");
            if (nRet == -1)
                return strText;
            string strLeft = strText.Substring(0, nRet).Trim();
            string strRight = strText.Substring(nRet + 1).Trim();
            return strRight + ", " + strLeft;
        }

        // 获得开头的拖干个拼音字头
        // 可能会抛出 ArgumentException 异常
        static string GetPinyinHead(string strText, int nCount)
        {
            string strResult = "";

            if (string.IsNullOrEmpty(strText) == true)
                return "";

            strText = strText.Trim();
            if (string.IsNullOrEmpty(strText) == true)
                return "";

            if (strText[0] == '(' || strText[0] == '（')
                throw new ArgumentException("拼音字符串 '" + strText + "' 中具有括号，不符合规范要求", "strText");

            string[] parts = strText.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in parts)
            {
                if (string.IsNullOrEmpty(s) == true)
                    continue;
                char ch = s[0];
                if (char.IsLetter(ch) == false)
                    continue;

                strResult += ch;
                if (strResult.Length >= nCount)
                    return strResult.ToUpper();
            }

            return strResult.ToUpper();
        }

        // 获得第一、第二个拼音字头
        // 如果有逗号，第二个拼音字头是指逗号后面的第一个
        static string GetShitoutangHead(string strText)
        {
            int nRet = strText.IndexOf(",");
            if (nRet != -1)
            {
                // 如果有逗号，则取两段的第一个拼音字头
                string strLeft = strText.Substring(0, nRet).Trim();
                string strRight = strText.Substring(nRet + 1).Trim();
                return GetPinyinHead(strLeft, 1) + GetPinyinHead(strRight, 1);
            }

            return GetPinyinHead(strText, 2);
        }

        // 为了取得石头汤著者号，把汉字转为拼音
        // 注: 拼音的大小写没有规整
        int HanziToPinyin(ref string strText,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strPinyin = "";

#if NO
            // 把字符串中的汉字和拼音分离
            // return:
            //      -1  出错
            //      0   用户希望中断
            //      1   正常
            if (string.IsNullOrEmpty(Program.MainForm.PinyinServerUrl) == true
               || Program.MainForm.ForceUseLocalPinyinFunc == true)
            {
                nRet = Program.MainForm.HanziTextToPinyin(
                    this.DetailForm,
                    true,	// 本地，快速
                    strText,
                    PinyinStyle.None,
                    out strPinyin,
                    out strError);
            }
            else
            {
                // 汉字字符串转换为拼音
                // 如果函数中已经MessageBox报错，则strError第一字符会为空格
                // return:
                //      -1  出错
                //      0   用户希望中断
                //      1   正常
                //      2   结果字符串中有没有找到拼音的汉字
                nRet = Program.MainForm.SmartHanziTextToPinyin(
                    this.DetailForm,
                    strText,
                    PinyinStyle.None,
                    false,  // auto sel
                    out strPinyin,
                    out strError);
            }
#endif
            nRet = Program.MainForm.GetPinyin(
                this.DetailForm,
                strText,
                PinyinStyle.None,
                false,
                out strPinyin,
                out strError);

            if (nRet == -1)
                return -1;
            if (nRet == 0)
            {
                strError = "用户中断。拼音子字段内容可能不完整。";
                return -1;
            }

            strText = strPinyin;
            return 0;
        }

        // 将汉字或者英文的著者字符串，取得石头汤著者号的两个字母
        int AutoGetShitoutangHead(ref string strText,
            out string strError)
        {
            strError = "";

            if (ContainHanzi(strText) == false)
            {
                strText = GetShitoutangHead(strText).ToUpper();
                return 0;
            }
            // 取拼音
            int nRet = HanziToPinyin(ref strText,
            out strError);
            if (nRet == -1)
                return -1;

            strText = GetShitoutangHead(strText).ToUpper();
            return 0;
        }

        // 把著者字符串正规化
        // 替换里面全角括号，逗号为半角形态
        // 可能会抛出 ArgumentException 异常
        static string CanonicalizeAuthorString(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return "";

            strText = strText.Trim();
            if (string.IsNullOrEmpty(strText) == true)
                return "";

            if (strText[0] == '(' || strText[0] == '（')
                throw new ArgumentException("著者字符串 '" + strText + "' 中首个字符为括号，不符合规范要求", "strText");


            return strText.Replace("（", "(").Replace("）", ")").Replace("，", ",");
        }

#if NO
        // 去掉外围的括号
        // (Hatzel, Richardson)
        static string Unquote(string strValue)
        {
            if (string.IsNullOrEmpty(strValue) == true)
                return "";
            if (strValue[0] == '(')
                strValue = strValue.Substring(1);
            if (string.IsNullOrEmpty(strValue) == true)
                return "";
            if (strValue[strValue.Length - 1] == ')')
                return strValue.Substring(0, strValue.Length - 1);

            return strValue;
        }
#endif

        // 2017/3/1
        // 获得一个著者字符串和对应的拼音
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        int GetAuthorAndPinyin(MarcRecord record,
            out string strAuthor,
            out string strPinyin,
            out string strError)
        {
            strError = "";
            strAuthor = "";
            strPinyin = "";

            MarcNodeList fields = record.select("field[@name='700' or @name='701' or @name='702' or @name='710' or @name='711' or @name='712']");
            fields.add(record.select("field[@name='200']")); // 必须两次分别 select。因为 200 一般在 MARC 记录中会先出现
            foreach (MarcNode field in fields)
            {
                if ((field.Name == "700" || field.Name == "701" || field.Name == "702")
                    && field.Indicator1 == 'A')
                    continue;

                string a = FirstContent(field.select("subfield[@name='a']"));
                if (string.IsNullOrEmpty(a))
                    continue;

                if (ContainHanzi(a) == false)
                    continue;

                // 看看是否有 &9
                string sub_9 = FirstContent(field.select("subfield[@name='9']"));
#if NO
                if (string.IsNullOrEmpty(sub_9) == false)
                {
                    strPinyin = a;
                    nRet = HanziToPinyin(ref a,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
                else
                    strPinyin = sub_9;
#endif
                strPinyin = sub_9;

                strAuthor = a;
                return 1;
            }

            return 0;
        }

        // 从选定的字段中获得石头汤著者字符串
        // TODO: 题名字符串不看 $b
        // return:
        //      -1  出错
        //      0   没有找到
        //      1   找到
        int GetShitoutangAuthorString(MarcNodeList fields,
            out string strAuthor,
            out string strError)
        {
            strAuthor = "";
            strError = "";
            int nRet = 0;

            foreach (MarcNode field in fields)
            {
#if NO
                // 先取得 $g
                string strText = FirstContent(field.select("subfield[@name='g']"));
                if (string.IsNullOrEmpty(strText) == false)
                {
                    strText = Unquote(strText);
                    strText = CanonicalizeAuthorString(strText);

                    // 观察指示符
                    if (field.Indicator2 == '1')
                        strAuthor = Reverse(strText);
                    else
                        strAuthor = strText;

                    // 将汉字或者英文的著者字符串，取得石头汤著者号的两个字母
                    nRet = AutoGetShitoutangHead(ref strAuthor,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    return 1;
                }
#endif

                // $a $b 同时具备的情况
                string a = FirstContent(field.select("subfield[@name='a']"));
                string b = FirstContent(field.select("subfield[@name='b']"));
                if (string.IsNullOrEmpty(a) == false && string.IsNullOrEmpty(b) == false)
                {
                    try
                    {
                        a = CanonicalizeAuthorString(a);
                        b = CanonicalizeAuthorString(b);
                    }
                    catch (ArgumentException ex)
                    {
                        strError = ex.Message;
                        return -1;
                    }

                    // 看看是否有 &9
                    string sub_9 = FirstContent(field.select("subfield[@name='9']"));
                    if (string.IsNullOrEmpty(sub_9) == false)
                    {
                        // 只得到 $b 的拼音部分
                        nRet = HanziToPinyin(ref b,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        strAuthor = sub_9 + ", " + b;

                        try
                        {
                            strAuthor = GetShitoutangHead(strAuthor);
                            return 1;
                        }
                        catch (ArgumentException)
                        {
                            // 继续当作没有拼音来处理，当时加入拼音
                        }
                    }
                    else
                    {
                        // 没有 $9
                        if (ContainHanzi(a) == true)
                        {
                            strError = "字段 " + field.Name + " 里面有 $a 而没有 $9，请先创建 $9 子字段";
                            return -1;
                        }
                    }

                    strAuthor = a + ", " + b;

                    // 将汉字或者英文的著者字符串，取得石头汤著者号的两个字母
                    nRet = AutoGetShitoutangHead(ref strAuthor,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    return 1;
                }

                // 只有 $a
                if (string.IsNullOrEmpty(a) == false)
                {
                    try
                    {
                        a = CanonicalizeAuthorString(a);
                    }
                    catch (ArgumentException ex)
                    {
                        strError = ex.Message;
                        return -1;
                    }

                    // 看看是否有 &9
                    string sub_9 = FirstContent(field.select("subfield[@name='9']"));
                    if (string.IsNullOrEmpty(sub_9) == false)
                    {
                        try
                        {
                            strAuthor = GetShitoutangHead(sub_9);
                            return 1;
                        }
                        catch (ArgumentException)
                        {
                            // 继续当作没有拼音来处理，当时加入拼音
                        }
                    }
                    else
                    {
                        // 没有 $9
                        if (ContainHanzi(a) == true)
                        {
                            strError = "字段 " + field.Name + " 里面有 $a 而没有 $9，请先创建 $9 子字段";
                            return -1;
                        }
                    }

                    strAuthor = a;

                    // 将汉字或者英文的著者字符串，取得石头汤著者号的两个字母
                    nRet = AutoGetShitoutangHead(ref strAuthor,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    return 1;
                }
            }

            return 0;   // 没有找到
        }

        #endregion

#endif

        // 获得索取号的第一行
        // parameters:
        //      strHeadLine 返回第一行的内容。返回null表示不要第一行。注：内容中不要包含{}指令。本函数的调用者会自动给加上
        // return:
        //      -1  error
        //      0   canceled
        //      1   succeed
        /// <summary>
        /// 获得索取号的第一行
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        /// <param name="index">下标</param>
        /// <param name="info">排架体系信息</param>
        /// <param name="strHeadLine">返回第一行。返回 null 表示不要第一行。注：内容中不要包含{}指令。本函数的调用者会自动给加上</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 放弃; 1: 成功</returns>
        public virtual int GetCallNumberHeadLine(
            object sender,
            GenerateDataEventArgs e,
            int index,
            ArrangementInfo info,
            out string strHeadLine,
            out string strError)
        {
            strError = "";
            /*
            strHeadLine = null;   // 缺省的效果为不需要第一行

            if (info != null)
            {
                if (string.IsNullOrEmpty(info.CallNumberStyle) == true
                    || info.CallNumberStyle == "索取类号+区分号"
                    || info.CallNumberStyle == "两行"
                    || info.CallNumberStyle == "二行")
                {
                    strHeadLine = null;
                    return 1;
                }
                else if (info.CallNumberStyle == "馆藏代码+索取类号+区分号"
                    || info.CallNumberStyle == "三行")
                {
                    strHeadLine = "{ns}馆藏代码";
                }
            }
             * */
            strHeadLine = "馆藏代码";

            return 1;
        }

#if NO
        // 获得索取号的第一行
        // parameters:
        //      strHeadLine 返回第一行的内容。返回null表示不要第一行
        // return:
        //      -1  error
        //      0   canceled
        //      1   succeed
        public virtual int GetCallNumberHeadLine(
            object sender,
            GenerateDataEventArgs e,
            int index,
            out string strHeadLine,
            out string strError)
        {
            strError = "";
            strHeadLine = null;   // 缺省的效果为不需要第一行

#if NOOOOOOOOOO
            int nRet = 0;
            string strLocation = "";
            string strClass = "";
            string strItemRecPath = "";

            EntityEditForm edit = null;
            EntityControl control = null;
            BookItem book_item = null;

            List<CallNumberItem> callnumber_items = null;

            string strBookType = "";

            if (sender is EntityEditForm)
            {
                edit = (EntityEditForm)sender;

                // 取得馆藏地点
                strLocation = edit.entityEditControl_editing.LocationString;
                strLocation = Global.GetPureLocation(strLocation);  // 2009/3/29 

                /*
                if (String.IsNullOrEmpty(strLocation) == true)
                {
                    strError = "请先输入馆藏地点。否则无法创建索取号";
                    goto ERROR1;
                }*/

                strHeadLine = GetHeadLinePart(edit.entityEditControl_editing.AccessNo);

                // 获得已有的类号
                strClass = GetClassPart(edit.entityEditControl_editing.AccessNo);

                strBookType = edit.entityEditControl_editing.BookType;

                strItemRecPath = edit.entityEditControl_editing.RecPath;

                callnumber_items = edit.BookItems.GetCallNumberItems();

                edit.Enabled = false;
                edit.Update();
            }
            else if (sender is EntityControl)
            {
                control = (EntityControl)sender;

                if (control.ListView.SelectedIndices.Count == 0)
                {
                    strError = "请先选定要关注的行。否则无法创建索取号";
                    return -1;
                }

                Debug.Assert(index >= 0 && index < control.ListView.SelectedIndices.Count, "");

                book_item = control.GetVisibleBookItemAt(control.ListView.SelectedIndices[index]);
                Debug.Assert(book_item != null, "");

                strLocation = book_item.Location;
                strLocation = Global.GetPureLocation(strLocation);  // 2009/3/29 

                /*
                if (String.IsNullOrEmpty(strLocation) == true)
                {
                    strError = "请先输入馆藏地点。否则无法创建索取号";
                    goto ERROR1;
                }*/

                strHeadLine = GetHeadLinePart(book_item.AccessNo);

                // 获得已有的类号
                strClass = GetClassPart(book_item.AccessNo);

                strBookType = book_item.BookType;

                strItemRecPath = book_item.RecPath;

                callnumber_items = control.BookItems.GetCallNumberItems();

                control.Enabled = false;
                control.Update();
            }
            else
            {
                strError = "sender必须是EntityEditForm或EntityControl类型(当前为" + sender.GetType().ToString() + ")";
                return -1;
            }

            try
            {
                string strArrangeGroupName = "";
                string strZhongcihaoDbname = "";
                string strClassType = "";
                string strQufenhaoType = "";

                // 获得关于一个特定馆藏地点的索取号配置信息
                nRet = Program.MainForm.GetCallNumberInfo(strLocation,
                    out strArrangeGroupName,
                    out strZhongcihaoDbname,
                    out strClassType,
                    out strQufenhaoType,
                    out strError);
                if (nRet == 0)
                {
                    strError = "没有关于馆藏地点 '" + strLocation + "' 的排架体系配置信息，无法获得索取号";
                    return -1;
                }
                if (nRet == -1)
                    return -1;


                // 工具书情况
                if (strBookType == "工具书")
                {
                    if (strClassType == "中图法")
                    {
                        strHeadLine = "(C)";
                        return 1;
                    }
                    else if (strClassType == "其它")
                    {
                        strHeadLine = "C";
                        return 1;
                    }
                    else
                    {
                        strError = "工具书类型时，无法处理的分类法 '" + strClassType + "'";
                        return -1;
                    }
                }

                // 不是工具书的情况

                // 从MARCEDIT中取得101$a
                string strLangCode = this.DetailForm.MarcEditor.Record.Fields.GetFirstSubfield(
                    "101",
                    "a");
                if (String.IsNullOrEmpty(strLangCode) == true)
                {
                    strError = "MARC记录中 101$a 没有找到，无法进行运算以便获得索取号第一行";
                    return -1;
                }

                /*
第一行	语种代码	注释
(1)	rus	俄文
(2)	eng	英文
(3)	ger	德文
(4)	fre	法文
(40892)		epo	世界语
chi	中文	如果是中文，则为空。
                 * 
可能需要考虑语种代码大小写不敏感的问题。
                 * */
                strLangCode = strLangCode.ToLower();
                if (strLangCode == "rus")
                {
                    strHeadLine = "(1)";
                    return 1;
                }
                if (strLangCode == "eng")
                {
                    strHeadLine = "(2)";
                    return 1;
                }
                if (strLangCode == "ger")
                {
                    strHeadLine = "(3)";
                    return 1;
                }
                if (strLangCode == "fre")
                {
                    strHeadLine = "(4)";
                    return 1;
                }
                if (strLangCode == "epo")
                {
                    strHeadLine = "(40892)";
                    return 1;
                } if (strLangCode == "chi")
                {
                    strHeadLine = "";
                    return 1;
                }

                strError = "因101$a语言代码为'" + strLangCode + "'，无法创建索取号第一行";
                return -1;
            }
            finally
            {
                if (sender is EntityEditForm)
                {
                    edit.Enabled = true;
                }
                else if (sender is EntityControl)
                {
                    control.Enabled = true;
                }
                else
                {
                    Debug.Assert(false, "");
                }
            }
#endif
            return 1;
        }
#endif

        // 创建一个索取号
        // return:
        //      -1  error
        //      0   canceled
        //      1   succeed
        /// <summary>
        /// 创建一个索取号
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        /// <param name="index">下标</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 放弃; 1: 成功</returns>
        public virtual int CreateOneCallNumber(
            object sender,
            GenerateDataEventArgs e,
            int index,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (sender == null)
            {
                strError = "sender为null";
                goto ERROR1;
            }

            string strLocation = "";
            string strClass = "";
            string strItemRecPath = "";

            BindingForm binding = null;
            EntityEditForm edit = null;
            EntityControl control = null;
            BookItem book_item = null;

            List<CallNumberItem> callnumber_items = null;

            ArrangementInfo info = null;

            Delegate_setText func_setText = null;
            Delegate_enableControls func_enableControls = null;

            if (sender is EntityEditForm)
            {
                edit = (EntityEditForm)sender;

                func_setText = (text) =>
                {
                    edit.entityEditControl_editing.AccessNo = text;
                };
                func_enableControls = (enable) =>
                    {
                        edit.Enabled = enable;
                        edit.Update();
                    };

                // 取得馆藏地点
                strLocation = edit.entityEditControl_editing.LocationString;
                strLocation = StringUtil.GetPureLocationString(strLocation);  // 2009/3/29 

#if NO
                // 获得关于一个特定馆藏地点的索取号配置信息
                nRet = Program.MainForm.GetArrangementInfo(strLocation,
                    out info,
                    out strError);
                if (nRet == 0)
                {
                    strError = "没有关于馆藏地点 '" + strLocation + "' 的排架体系配置信息，无法获得索取号";
                    goto ERROR1;
                }
                if (nRet == -1)
                    goto ERROR1;

                // 获得已有的类号
                strClass = GetClassPart(
                    info.CallNumberStyle,
                    edit.entityEditControl_editing.AccessNo);
#endif
                strClass = edit.entityEditControl_editing.AccessNo;

                strItemRecPath = edit.entityEditControl_editing.RecPath;
#if REF
                // 2017/4/6
                if (string.IsNullOrEmpty(strItemRecPath))
                {
                    strItemRecPath = "@refID:" + edit.entityEditControl_editing.RefID;
                    // TODO: 不应为空
                }
#endif
                // callnumber_items = edit.BookItems.GetCallNumberItems();
                callnumber_items = edit.GetCallNumberItems();

#if NO
                edit.Enabled = false;
                edit.Update();
#endif
            }
            else if (sender is EntityControl)
            {
                control = (EntityControl)sender;

                func_enableControls = (enable) =>
                    {
                        control.Enabled = enable;
                        control.Update();
                    };

                if (control.ListView.SelectedIndices.Count == 0)
                {
                    strError = "请先选定要关注的行。否则无法创建索取号";
                    goto ERROR1;
                }

                Debug.Assert(index >= 0 && index < control.ListView.SelectedIndices.Count, "");

                book_item = control.GetVisibleItemAt(control.ListView.SelectedIndices[index]);
                Debug.Assert(book_item != null, "");

                func_setText = (text) =>
                    {
                        book_item.AccessNo = text;
                        book_item.RefreshListView();
                        // 2011/11/10
                        EntityControl entity_control = (EntityControl)sender;
                        entity_control.Changed = true;
                    };

                strLocation = book_item.Location;
                strLocation = StringUtil.GetPureLocationString(strLocation);  // 2009/3/29 

#if NO
                // 获得关于一个特定馆藏地点的索取号配置信息
                nRet = Program.MainForm.GetArrangementInfo(strLocation,
                    out info,
                    out strError);
                if (nRet == 0)
                {
                    strError = "没有关于馆藏地点 '" + strLocation + "' 的排架体系配置信息，无法获得索取号";
                    goto ERROR1;
                }
                if (nRet == -1)
                    goto ERROR1;

                // 获得已有的类号
                strClass = GetClassPart(
                    info.CallNumberStyle,
                    book_item.AccessNo);
#endif
                strClass = book_item.AccessNo;

                strItemRecPath = book_item.RecPath;

#if REF
                // 2017/4/6
                if (string.IsNullOrEmpty(strItemRecPath))
                {
                    strItemRecPath = "@refID:" + book_item.RefID;
                    // TODO: 不应为空
                }
#endif
                callnumber_items = control.Items.GetCallNumberItems();
            }
            else if (sender is BindingForm)
            {
                binding = (BindingForm)sender;

                func_enableControls = (enable) =>
                    {
                        binding.Enabled = enable;
                        binding.Update();
                    };

                func_setText = (text) =>
                    {
                        binding.EntityEditControl.AccessNo = text;
                    };

                // 取得馆藏地点
                strLocation = binding.EntityEditControl.LocationString;
                strLocation = StringUtil.GetPureLocationString(strLocation);  // 2009/3/29 

#if NO
                // 获得关于一个特定馆藏地点的索取号配置信息
                nRet = Program.MainForm.GetArrangementInfo(strLocation,
                    out info,
                    out strError);
                if (nRet == 0)
                {
                    strError = "没有关于馆藏地点 '" + strLocation + "' 的排架体系配置信息，无法获得索取号";
                    goto ERROR1;
                }
                if (nRet == -1)
                    goto ERROR1;

                // 获得已有的类号
                strClass = GetClassPart(
                    info.CallNumberStyle,
                    binding.EntityEditControl.AccessNo);
#endif
                strClass = binding.EntityEditControl.AccessNo;

                strItemRecPath = binding.EntityEditControl.RecPath;

#if REF
                // 2017/4/6
                if (string.IsNullOrEmpty(strItemRecPath))
                {
                    strItemRecPath = "@refID:" + binding.EntityEditControl.RefID;
                    // TODO: 不应为空
                }
#endif
                // callnumber_items = edit.BookItems.GetCallNumberItems();
                callnumber_items = binding.GetCallNumberItems();
            }
            else if (sender is BiblioAndEntities && e.Parameter is GetCallNumberParameter)
            {
                BiblioAndEntities biblio = sender as BiblioAndEntities;
                callnumber_items = biblio.GetCallNumberItems();
                GetCallNumberParameter parameter = e.Parameter as GetCallNumberParameter;

                func_enableControls = (enable) =>
                    {
                        biblio.Owner.Enabled = enable;
                    };
                func_setText = (text) =>
                    {
                        parameter.ResultAccessNo = text;
                    };

                strLocation = parameter.Location;
                strClass = parameter.ExistingAccessNo;
                strItemRecPath = parameter.RecPath;
#if NO
                string strResult = "";
                nRet = CreateOneCallNumber(
                    this.Form,
                    callnumber_items,
                    parameter.ExistingAccessNo,
                    parameter.Location,
                    parameter.RecPath,
                    out strResult,
                    out strError);
                if (nRet == -1)
                    e.ErrorInfo = strError;
                else
                    parameter.ResultAccessNo = strResult;
                return;
#endif
            }

            else if (sender is BiblioAndEntities)
            {
                BiblioAndEntities biblio = sender as BiblioAndEntities;
                callnumber_items = biblio.GetCallNumberItems();
                EntityEditControl edit0 = e.FocusedControl as EntityEditControl;

                func_enableControls = (enable) =>
                {
                    biblio.Owner.Enabled = enable;
                };
                func_setText = (text) =>
                {
                    edit0.Text = text;
                };
                strLocation = edit0.LocationString;
                strClass = edit0.AccessNo;

                strItemRecPath = edit0.RecPath;
#if NO
                string strResult = "";
                nRet = CreateOneCallNumber(
                    this.Form,
                    callnumber_items,
                    edit.AccessNo,
                    edit.LocationString,
                    edit.RecPath,
                    out strResult,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                edit.AccessNo = strResult;
#endif
            }
            else
            {
                strError = "sender必须是EntityEditForm或EntityControl或BindingForm类型(当前为" + sender.GetType().ToString() + ")";
                goto ERROR1;
            }

            if (info == null)
            {
                // 获得关于一个特定馆藏地点的索取号配置信息
                nRet = Program.MainForm.GetArrangementInfo(strLocation,
                    out info,
                    out strError);
                if (nRet == 0)
                {
                    strError = "没有关于馆藏地点 '" + strLocation + "' 的排架体系配置信息，无法获得索取号";
                    goto ERROR1;
                }
                if (nRet == -1)
                    goto ERROR1;
            }

            // 获得已有的类号
            strClass = GetClassPart(
                info.CallNumberStyle,
                strClass);

            func_enableControls(false);
            try
            {
                string strHeadLine = null;

                if (info.CallNumberStyle == "馆藏代码+索取类号+区分号"
                    || info.CallNumberStyle == "三行")
                {
                    // 获得索取号的第一行
                    // return:
                    //      -1  error
                    //      0   canceled
                    //      1   succeed
                    nRet = GetCallNumberHeadLine(
                        sender,
                        e,
                        index,
                        info,
                        out strHeadLine,
                        out strError);
                    if (nRet == -1 || nRet == 0)
                        return nRet;

                    if (strHeadLine != null)
                        strHeadLine = "{ns}" + strHeadLine;
                }

                // 总是从MARCEDIT中取得索取类号

                // (从MARC编辑器中)获得索取号类号部分
                // return:
                //      -1  error
                //      0   MARC记录中没有找到所需的来源字段/子字段内容
                //      1   succeed
                nRet = GetCallNumberClassPart(
                    info.ClassType,
                    out strClass,
                    out strError);
                if (nRet == -1)
                {
                    strError = "获得索取类号时出错: " + strError;
                    goto ERROR1;
                }
                if (nRet == 0)
                {
                    goto ERROR1;
                }

                Debug.Assert(nRet == 1, "");

                // 先设置已经获得的索取类号部分
                func_setText((strHeadLine != null ? strHeadLine + "/" : "")
                        + strClass);

#if NO
                // 先设置已经获得的索取类号部分
                if (sender is EntityEditForm)
                {
                    edit.entityEditControl_editing.AccessNo =
                        (strHeadLine != null ? strHeadLine + "/" : "")
                        + strClass;
                }
                else if (sender is EntityControl)
                {
                    // book_item.AccessNo = strClass;

                    book_item.AccessNo =
    (strHeadLine != null ? strHeadLine + "/" : "")
    + strClass;


                    book_item.RefreshListView();

                    // 2011/11/10
                    EntityControl entity_control = (EntityControl)sender;
                    entity_control.Changed = true;
                }
                else if (sender is BindingForm)
                {
                    binding.EntityEditControl.AccessNo =
                        (strHeadLine != null ? strHeadLine + "/" : "")
                        + strClass;
                }
                else
                {
                    Debug.Assert(false, "");
                }
#endif

                string strQufenhao = "";

                if (info.QufenhaoType == "zhongcihao"
                    || info.QufenhaoType == "种次号")
                {
#if NO
                    if (StringUtil.CompareVersion(Program.MainForm.ServerVersion, "2.104") < 0)
                    {
                        strError = "创建种次号功能必须和 dp2library 2.104 及以上版本配套使用 (而当前连接的 dp2library 版本为 "+Program.MainForm.ServerVersion+")";
                        goto ERROR1;
                    }
#endif

                    // 获得种次号
                    CallNumberForm dlg = new CallNumberForm();

                    try
                    {
                        dlg.MainForm = Program.MainForm;
                        // dlg.TopMost = true;
                        if (sender is Form)
                            dlg.Owner = (Form)sender;
                        dlg.MyselfItemRecPath = strItemRecPath;
                        dlg.MyselfParentRecPath = this.DetailForm.BiblioRecPath;
                        dlg.MyselfCallNumberItems = callnumber_items;   // 2009/6/4 

                        Debug.Assert(this.DetailForm.MemoNumbers != null, "");
                        dlg.MemoNumbers = this.DetailForm.MemoNumbers;

                        dlg.Show();

                        ZhongcihaoStyle style = ZhongcihaoStyle.Seed;

                        if (String.IsNullOrEmpty(info.ZhongcihaoDbname) == true)
                            style = ZhongcihaoStyle.Biblio; // 没有配置种次号库，只好从书目数据库中统计获得最大号
                        else
                        {
                            style = ZhongcihaoStyle.BiblioAndSeed;   // 有了尾号库，就用书目+尾号

                            // style = ZhongcihaoStyle.Seed;   // 有了尾号库，就用尾号
                        }

                        // TODO: 这里尽量用数据中统计出来的最大号，而不用尾号
                        // 需要注意，如果同一书目记录中已经有了一个种次号，就直接用它
                        // 如果要用Seed风格，可以实现在autogen脚本中，而不在这里实现

                        // return:
                        //      -1  error
                        //      0   canceled
                        //      1   succeed
                        nRet = dlg.GetZhongcihao(
                            style,
                            strClass,
                            strLocation,
                            out strQufenhao,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == 0)
                            return 0;
                    }
                    catch (Exception ex)
                    {
                        strError = "DetailHost {3D00C7E0-9E54-43D4-B77A-84BCC91BE45A} exception: " + ExceptionUtil.GetAutoText(ex);
                        goto ERROR1;
                    }
                    finally
                    {
                        dlg.Close();
                    }
                }
                else
                {
                    // 获得种次号以外的其他区分号
                    // return:
                    //      -1  error
                    //      0   not found。注意此时也要设置strError值
                    //      1   found
                    nRet = GetAuthorNumber(info.QufenhaoType,
                        out strQufenhao,
                        out strError);
                    if (nRet == -1 || nRet == 0)
                        goto ERROR1;
                }

                // 最后设置完整的索取类号
                func_setText((strHeadLine != null ? strHeadLine + "/" : "")
                        + strClass +
                        (string.IsNullOrEmpty(strQufenhao) == false ?
                        "/" + strQufenhao : ""));

#if NO
                // 最后设置完整的索取类号
                if (sender is EntityEditForm)
                {
                    edit.entityEditControl_editing.AccessNo =
                        (strHeadLine != null ? strHeadLine + "/" : "")
                        + strClass +
                        (string.IsNullOrEmpty(strQufenhao) == false ?
                        "/" + strQufenhao : "");
                }
                else if (sender is EntityControl)
                {
                    book_item.AccessNo =
                        (strHeadLine != null ? strHeadLine + "/" : "")
                        + strClass +
                        (string.IsNullOrEmpty(strQufenhao) == false ?
                        "/" + strQufenhao : "");
                    book_item.RefreshListView();
                    // 2011/11/10
                    EntityControl entity_control = (EntityControl)sender;
                    entity_control.Changed = true;
                }
                else if (sender is BindingForm)
                {
                    binding.EntityEditControl.AccessNo =
                        (strHeadLine != null ? strHeadLine + "/" : "")
                        + strClass +
                        (string.IsNullOrEmpty(strQufenhao) == false ?
                        "/" + strQufenhao : "");
                }
                else
                {
                    Debug.Assert(false, "");
                }
#endif
            }
            finally
            {
#if NO
                if (sender is EntityEditForm)
                {
                    edit.Enabled = true;
                }
                else if (sender is EntityControl)
                {
                    control.Enabled = true;
                }
                else if (sender is BindingForm)
                {
                    binding.Enabled = true;
                }
                else
                {
                    Debug.Assert(false, "");
                }
#endif
                func_enableControls(true);
            }

            return 1;
            ERROR1:
            e.ErrorInfo = strError;
            return -1;
        }

        delegate void Delegate_setText(string strText);

        delegate void Delegate_enableControls(bool bEnable);

        // 创建索取号
        /// <summary>
        /// 创建索取号
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        public virtual void CreateCallNumber(object sender,
            GenerateDataEventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (sender == null)
            {
                strError = "sender为null";
                goto ERROR1;
            }
            if (sender is EntityEditForm)
            {
                nRet = CreateOneCallNumber(sender,
                    e,
                    0,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            else if (sender is EntityControl)
            {
                EntityControl control = (EntityControl)sender;
                if (control.ListView.SelectedIndices.Count == 0)
                {
                    strError = "请先选定要关注的行。否则无法创建索取号";
                    goto ERROR1;
                }

                for (int i = 0; i < control.ListView.SelectedIndices.Count; i++)
                {
                    nRet = CreateOneCallNumber(sender,
                        e,
                        i,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    // 放弃回答问题。是否视为中断操作的意思?
                    if (nRet == 0)
                    {
                        strError = "中途放弃";
                        goto ERROR1;
                    }
                }
            }
            else if (sender is BindingForm)
            {
                nRet = CreateOneCallNumber(sender,
                    e,
                    0,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            else if (sender is BiblioAndEntities)
            {
                nRet = CreateOneCallNumber(sender,
                    e,
                    0,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            else
            {
                strError = "sender必须是EntityEditForm或EntityControl或BindingForm类型(当前为" + sender.GetType().ToString() + ")";
                goto ERROR1;
            }
            return;
            ERROR1:
            e.ErrorInfo = strError;
            if (e.ShowErrorBox == true)
            {
                // MessageBox.Show(this.DetailForm, strError);
                bool bTemp = false;
                // TODO: 如果保持窗口修改后的尺寸位置?
                MessageDialog.Show(this.DetailForm,
                    "创建索取号时出错",
                    strError,
                    null,
                    ref bTemp);
            }
        }

        // GCAT通道登录
        internal void gcat_channel_BeforeLogin(object sender,
            DigitalPlatform.GcatClient.BeforeLoginEventArgs e)
        {
            string strUserName = (string)Program.MainForm.ParamTable["author_number_account_username"];
            string strPassword = (string)Program.MainForm.ParamTable["author_number_account_password"];

            if (String.IsNullOrEmpty(strUserName) == true)
            {
                strUserName = "test";
                strPassword = "";
            }

            // 直接试探
            if (!(e.UserName == strUserName && e.Failed == true)
                && strUserName != "")
            {
                e.UserName = strUserName;
                e.Password = strPassword;
                return;
            }

            LoginDlg dlg = new LoginDlg();
            GuiUtil.SetControlFont(dlg, Program.MainForm.DefaultFont, false);

            if (e.Failed == true)
                dlg.textBox_comment.Text = "登录失败。加著者号码功能需要重新登录";
            else
                dlg.textBox_comment.Text = "加著者号码功能需要登录";

            dlg.textBox_serverAddr.Text = e.GcatServerUrl;
            dlg.textBox_userName.Text = strUserName;
            dlg.textBox_password.Text = strPassword;
            dlg.checkBox_savePassword.Checked = true;

            dlg.textBox_serverAddr.Enabled = false;
            dlg.TopMost = true; // 2009/11/12  因为ShowDialog(null)，为了防止对话框被放在非顶部
            dlg.ShowDialog(null);
            if (dlg.DialogResult != DialogResult.OK)
            {
                e.Cancel = true;    // 2009/11/12  如果缺这一句，会造成Cancel后仍然重新弹出登录对话框
                return;
            }

            strUserName = dlg.textBox_userName.Text;
            strPassword = dlg.textBox_password.Text;

            e.UserName = strUserName;
            e.Password = strPassword;

            Program.MainForm.ParamTable["author_number_account_username"] = strUserName;
            Program.MainForm.ParamTable["author_number_account_password"] = strPassword;
        }

        /// <summary>
        /// 通过资源 ID 找到对应的 856 字段
        /// </summary>
        /// <param name="editor">MARC 编辑器</param>
        /// <param name="strID">资源 ID</param>
        /// <returns>字段对象集合</returns>
        public static List<Field> Find856ByResID(MarcEditor editor,
            string strID)
        {
            List<Field> results = new List<Field>();

            for (int i = 0; i < editor.Record.Fields.Count; i++)
            {
                Field field = editor.Record.Fields[i];

                if (field.Name == "856")
                {
                    // 找到$8
                    for (int j = 0; j < field.Subfields.Count; j++)
                    {
                        Subfield subfield = field.Subfields[j];
                        if (subfield.Name == LinkSubfieldName)
                        {
                            string strValue = subfield.Value;
                            if (StringUtil.HasHead(strValue, "uri:") == true)
                                strValue = strValue.Substring("uri:".Length);

                            if (strValue == strID)
                            {
                                results.Add(field);
                            }
                        }
                    }
                }
            }

            return results;
        }

        public static string LinkSubfieldName = "u";

        /// <summary>
        /// 管理 856 字段
        /// </summary>
        /// <param name="sender">事件触发者</param>
        /// <param name="e">事件参数</param>
        public virtual void Manage856(object sender,
            GenerateDataEventArgs e)
        {
            string strID = "";
            Field field_856 = null;

            string SUBFLD = new String((char)31, 1);

            // 如果功能从对象控件发起
            // 需要把和对象控件中当前选择行id有关的856字段信息装入，进行管理
            if (sender is BinaryResControl)
            {
                BinaryResControl control = (BinaryResControl)sender;
                if (control.ListView.SelectedIndices.Count >= 1)
                {
                    // 获得当前选中行的id
                    strID = ListViewUtil.GetItemText(control.ListView.SelectedItems[0], 0);
                }
            }

            // 在MARC编辑器中找到已有id的856字段
            if (String.IsNullOrEmpty(strID) == false)
            {
                List<Field> fields = Find856ByResID(DetailForm.MarcEditor,
                    strID);

                if (fields.Count == 1)
                    field_856 = fields[0];
                else if (fields.Count > 1)
                {
                    DialogResult result = MessageBox.Show(this.DetailForm,
                        "当前MARC编辑器中已经存在 " + fields.Count.ToString() + " 个856字段其$" + LinkSubfieldName + "子字段关联了对象ID '" + strID + "' ，是否要编辑其中的第一个856字段?\r\n\r\n(注：可改在MARC编辑器中选中一个具体的856字段进行编辑)\r\n\r\n(OK: 编辑其中的第一个856字段; Cancel: 取消操作",
                        "DetailHost",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.Cancel)
                        return;
                    field_856 = fields[0];
                }
            }

            // 如果发起来自MARC编辑器
            if (field_856 == null
                && !(sender is BinaryResControl))
            {
                // 看看当前活动字段是不是856
                field_856 = this.DetailForm.MarcEditor.FocusedField;
                if (field_856 != null)
                {
                    if (field_856.Name != "856")
                        field_856 = null;
                }
            }

            if (field_856 != null)
            {
                this.DetailForm.MarcEditor.FocusedField = field_856;
                this.DetailForm.MarcEditor.EnsureVisible();
            }

            Field856Dialog dlg = new Field856Dialog();
            GuiUtil.SetControlFont(dlg, Program.MainForm.DefaultFont, false);
            dlg.RightsCfgFileName = Path.Combine(Program.MainForm.UserDir, "objectrights.xml");
            dlg.MarcSyntax = DetailForm.MarcSyntax;
            dlg.GetResInfo -= new GetResInfoEventHandler(dlg_GetResInfo);
            dlg.GetResInfo += new GetResInfoEventHandler(dlg_GetResInfo);

            if (field_856 != null)
            {
                dlg.Text = "修改856字段";
                dlg.Value = field_856.IndicatorAndValue;
            }
            else
            {
                dlg.Text = "创建新的856字段";
                if (DetailForm.MarcSyntax == "unimarc")
                    dlg.Value = "7 ";
                else
                    dlg.Value = "72";   // 缺省值

                if (String.IsNullOrEmpty(strID) == false)
                {
                    dlg.Value += SUBFLD + LinkSubfieldName + strID + SUBFLD + dlg.GetAccessMethodSubfieldName() + "dp2res";
                    dlg.AutoFollowIdSet = true; // 连带填充其它几个子字段
                    dlg.MessageText = "尚不存在和对象ID '" + strID + "' 关联的856字段，现在请创建...";
                }
            }

            REDO_INPUT:
            Program.MainForm.AppInfo.LinkFormState(dlg, "ctrl_a_field856dialog_state");
            dlg.ShowDialog(this.DetailForm);
            Program.MainForm.AppInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            // 新创建情况下，检查一下 id 是否已经存在，给与适当警告
            if (field_856 == null
                && String.IsNullOrEmpty(dlg.Subfield_u) == false)
            {
                List<Field> dup_fields = Find856ByResID(DetailForm.MarcEditor, dlg.Subfield_u);

                if (dup_fields.Count > 0)
                {
                    DialogResult result = MessageBox.Show(this.DetailForm,
                        "当前 MARC 编辑器中已经存在 " + dup_fields.Count + " 个 856 字段其 $" + LinkSubfieldName + " 子字段关联了对象ID '" + dlg.Subfield_u + "' ，确实要再次新创建一个关联此对象 ID 的新 856 字段?\r\n\r\n(注：如果必要，多个856字段是可以关联同一对象ID的)\r\n\r\n(Yes: 立即创建; No: 不创建，关闭对话框，输入的内容丢失; Cancel: 重新打开对话框以便进一步修改)",
                        "DetailHost",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button3);
                    if (result == DialogResult.No)
                        return;
                    if (result == DialogResult.Cancel)
                        goto REDO_INPUT;
                }
            }

            {
                // 如果当前活动字段不是856，则创建一个新的856字段
                if (field_856 == null)
                {
                    // this.DetailForm.MarcEditor.Flush();
                    field_856 = this.DetailForm.MarcEditor.Record.Fields.Add("856", "  ", "", true);
                }

                field_856.IndicatorAndValue = dlg.Value;
                this.DetailForm.MarcEditor.EnsureVisible();

                // 修改对象的 rights
                if (string.IsNullOrEmpty(dlg.Subfield_u) == false)
                {
                    this.DetailForm.ChangeObjectRigths(dlg.Subfield_u, dlg.ObjectRights);
                }
            }
        }

        void dlg_GetResInfo(object sender, GetResInfoEventArgs e)
        {
            this.DetailForm.GetResInfo(sender, e);
        }

        int SearchDictionary(
            LibraryChannel channel,
    Stop stop,
    string strDbName,
    string strKey,
    string strMatchStyle,
    int nMaxCount,
    ref List<string> results,
    out string strError)
        {
            return Program.MainForm.SearchDictionary(
                channel,
            stop,
            strDbName,
            strKey,
            strMatchStyle,
            nMaxCount,
            ref results,
            out strError);
        }

#if NO
        void DoStop(object sender, StopEventArgs e)
        {
            if (Program.MainForm.Channel != null)
                Program.MainForm.Channel.Abort();
        }
#endif

        // 通过词典库对照关系创建新字段
        // parameters:
        //      strDef  关系定义。例如 "dbname=LCC-CLC,source=050a,target=098a,color=#00aa00;dbname=DDC-CLC,source=082a,target=098a,color=#aaaa00"
        //      strDefaultStyle 缺省的处理风格。如果为空，表示不改变 RelationDialog 本身的缺省处理风格，即 DefaultStyle 值。
        public void RelationGenerate(string strDef,
            string strDefaultStyle = "")
        {
            string strError = "";

            RelationCollection relations = new RelationCollection();
            int nRet = relations.Build(this.DetailForm.MarcEditor.Marc, strDef, out strError);
            if (nRet == -1)
                goto ERROR1;

            LibraryChannel channel = Program.MainForm.GetChannel();

            try
            {
                RelationDialog dlg = new RelationDialog();
                MainForm.SetControlFont(dlg, this.DetailForm.Font, false);
                if (string.IsNullOrEmpty(strDefaultStyle) == false)
                    dlg.DefaultStyle = strDefaultStyle;
                dlg.Channel = channel;
                dlg.ProcSearchDictionary = SearchDictionary;
#if NO
            dlg.ProcDoStop = (sender, e) => {
                channel.Abort();
            };
#endif
                dlg.TempDir = Program.MainForm.UserTempDir;
                dlg.MarcHtmlHead = Program.MainForm.GetMarcHtmlHeadString();
                dlg.RelationCollection = relations;
                dlg.UiState = Program.MainForm.AppInfo.GetString(
                    "RelationDialog",
                    "ui_state",
                    "");
                Program.MainForm.AppInfo.LinkFormState(dlg, "SelectDictionaryItemDialog_state");
                dlg.ShowDialog(this.DetailForm);
                Program.MainForm.AppInfo.SetString(
                    "RelationDialog",
                    "ui_state",
                    dlg.UiState);
                if (dlg.DialogResult != DialogResult.OK)
                    return;

                this.DetailForm.MarcEditor.Marc = dlg.OutputMARC;

            }
            finally
            {
                Program.MainForm.ReturnChannel(channel);
            }
#if NO
            foreach (string s in dlg.ResultRelations)
            {
                Field target_field = null;

                // target_field = this.DetailForm.MarcEditor.Record.Fields.GetOneField(strTargetFieldName, 0);

                if (target_field == null)
                {
                    target_field = this.DetailForm.MarcEditor.Record.Fields.Add(strTargetFieldName, "  ", "", true);
                }

                Subfield target_subfield = target_field.Subfields[strTargetSubfieldName];
                if (target_subfield == null)
                {
                    target_subfield = new Subfield();
                    target_subfield.Name = strTargetSubfieldName;
                }

                target_subfield.Value = s;
                target_field.Subfields[strTargetSubfieldName] = target_subfield;
            }
#endif
            this.DetailForm.MarcEditor.EnsureVisible();
            return;
            ERROR1:
            MessageBox.Show(this.DetailForm, strError);
        }
    }


    /// <summary>
    /// 拼音配置事项
    /// </summary>
    public class PinyinCfgItem
    {
        /// <summary>
        /// 字段名
        /// </summary>
        public string FieldName = "";

        /// <summary>
        /// 指示符匹配方式
        /// </summary>
        public string IndicatorMatchCase = "";

        /// <summary>
        /// 从什么子字段。每个字符表示一个子字段名
        /// </summary>
        public string From = "";

        /// <summary>
        /// 从什么子字段。每个字符表示一个子字段名
        /// 到什么子字段。
        /// </summary>
        public string To = "";

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="nodeItem">一个元素节点</param>
        public PinyinCfgItem(XmlNode nodeItem)
        {
            this.FieldName = DomUtil.GetAttr(nodeItem, "name");
            this.IndicatorMatchCase = DomUtil.GetAttr(nodeItem, "indicator");
            this.From = DomUtil.GetAttr(nodeItem, "from");
            this.To = DomUtil.GetAttr(nodeItem, "to");
        }
    }
}
