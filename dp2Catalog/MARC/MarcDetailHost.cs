using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.Marc;
using DigitalPlatform.Script;
using DigitalPlatform.GcatClient;
using DigitalPlatform.Text;
using DigitalPlatform.IO;
using DigitalPlatform.CommonControl;

using DigitalPlatform.CirculationClient;
using DigitalPlatform.GUI;

namespace dp2Catalog
{
    /// <summary>
    /// Summary description for Host.
    /// </summary>
    public class MarcDetailHost
    {
        public MarcDetailForm DetailForm = null;
        public Assembly Assembly = null;
        public ScriptActionCollection ScriptActions = new ScriptActionCollection();

        public MarcDetailHost()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public void Invoke(string strFuncName)
        {
            Type classType = this.GetType();

            // new一个Host派生对象
            classType.InvokeMember(strFuncName,
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.InvokeMethod
                ,
                null,
                this,
                null);

        }

        public void Invoke(string strFuncName,
    object sender,
    GenerateDataEventArgs e)
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

            if (strFuncName == "Main")
            {
                classType = this.GetType();

                // 老的HostEventArgs e 参数
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
                            new object[] { sender, new HostEventArgs() });
                        return;

                    }
                    catch (System.MissingMethodException/*ex*/)
                    {
                        classType = classType.BaseType;
                        if (classType == null)
                            break;
                    }
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

        public virtual void Main(object sender, HostEventArgs e)
        {

        }

        // parameters:
        //      strIndicator    字段指示符。如果用null调用，则表示不对指示符进行筛选
        // return:
        //      0   没有找到匹配的配置事项
        //      >=1 找到。返回找到的配置事项个数
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

        // 包装后的版本，兼容以前的调用习惯
        public int AddPinyin(string strCfgXml,
    bool bUseCache = true,
    PinyinStyle style = PinyinStyle.None,
    string strPrefix = "",
        bool bAutoSel = false)
        {
            return AddPinyin(strCfgXml,
                bUseCache,
                style,
                strPrefix,
                bAutoSel ? "auto" : "");
        }

        /// <summary>
        /// 为 MARC 编辑器内的记录加拼音
        /// </summary>
        /// <param name="strCfgXml">拼音配置 XML</param>
        /// <param name="bUseCache">是否使用记录中以前缓存的结果？</param>
        /// <param name="style">风格</param>
        /// <param name="strPrefix">前缀字符串。缺省为空 [暂时没有使用本参数]</param>
        /// <param name="strDuoyinStyle">是否自动选择多音字。auto/first 之一或者组合</param>
        /// <returns>-1: 出错。包括中断的情况; 0: 正常</returns>
        public virtual int AddPinyin(string strCfgXml,
            bool bUseCache, //  = true,
            PinyinStyle style,  // = PinyinStyle.None,
            string strPrefix,
            string strDuoyinStyle)
            // bool bAutoSel = false)
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
                                if (MarcDetailHost.ContainHanzi(strHanzi) == false)
                                    continue;

                                string strPinyin;

                                strPinyin = (string)old_selected[strHanzi];
                                if (string.IsNullOrEmpty(strPinyin) == true)
                                {

                                    // 把字符串中的汉字和拼音分离
                                    // return:
                                    //      -1  出错
                                    //      0   用户希望中断
                                    //      1   正常
                                    if (string.IsNullOrEmpty(this.DetailForm.MainForm.PinyinServerUrl) == true
                                       || this.DetailForm.MainForm.ForceUseLocalPinyinFunc == true)
                                    {
                                        nRet = this.DetailForm.MainForm.HanziTextToPinyin(
                                            this.DetailForm,
                                            true,	// 本地，快速
                                            strHanzi,
                                            style,
                                            strDuoyinStyle,
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
                                        nRet = this.DetailForm.MainForm.SmartHanziTextToPinyin(
                                            this.DetailForm,
                                            strHanzi,
                                            style,
                                            strDuoyinStyle,
                                            out strPinyin,
                                            out strError);
                                    }
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
                                nRet = MarcUtil.InsertSubfield(
                                    ref strField,
                                    from,
                                    j,
                                    new string(MarcUtil.SUBFLD, 1) + to + strPinyin,
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

        public virtual void RemovePinyin(string strCfgXml)
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
                    bool bChanged = false;
                    foreach (PinyinCfgItem item in cfg_items)
                    {
                        for (int k = 0; k < item.To.Length; k++)
                        {
                            string to = new string(item.To[k], 1);
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

        DigitalPlatform.GcatClient.Channel GcatChannel = null;

        // 获得著者号
        // return:
        //      -1  error
        //      0   canceled
        //      1   succeed
        public int GetGcatAuthorNumber(string strGcatWebServiceUrl,
            string strAuthor,
            out string strAuthorNumber,
            out string strError)
        {
            strError = "";
            strAuthorNumber = "";

            if (String.IsNullOrEmpty(strGcatWebServiceUrl) == true)
                strGcatWebServiceUrl = "http://dp2003.com/gcatserver/";  //  "http://dp2003.com/dp2libraryws/gcat.asmx";

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
                        this.DetailForm.stop,
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
            else
            {
                // 新的WebService

                string strID = this.DetailForm.MainForm.AppInfo.GetString("DetailHost", "gcat_id", "");
                bool bSaveID = this.DetailForm.MainForm.AppInfo.GetBoolean("DetailHost", "gcat_saveid", false);

                Hashtable question_table = (Hashtable)this.DetailForm.MainForm.ParamTable["question_table"];
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
                        this.DetailForm.stop,
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
                        GuiUtil.AutoSetDefaultFont(login_dlg);
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
                            this.DetailForm.MainForm.AppInfo.SetString("DetailHost", "gcat_id", strID);
                        }
                        else
                        {
                            this.DetailForm.MainForm.AppInfo.SetString("DetailHost", "gcat_id", "");
                        }
                        this.DetailForm.MainForm.AppInfo.SetBoolean("DetailHost", "gcat_saveid", bSaveID);
                        goto REDO_GETNUMBER;
                    }

                    this.DetailForm.MainForm.ParamTable["question_table"] = question_table;

                    return nRet;
                }
                finally
                {
                    EndGcatLoop();
                }
            }
        }

        // GCAT通道登录 旧的方式
        public void gcat_channel_BeforeLogin(object sender,
            DigitalPlatform.GcatClient.BeforeLoginEventArgs e)
        {
            string strUserName = (string)this.DetailForm.MainForm.ParamTable["author_number_account_username"];
            string strPassword = (string)this.DetailForm.MainForm.ParamTable["author_number_account_password"];

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
            GuiUtil.SetControlFont(dlg, this.DetailForm.MainForm.Font);

            if (e.Failed == true)
                dlg.textBox_comment.Text = "登录失败。加著者号码功能需要重新登录";
            else
                dlg.textBox_comment.Text = "加著者号码功能需要登录";

            dlg.textBox_serverAddr.Text = e.GcatServerUrl;
            dlg.textBox_userName.Text = strUserName;
            dlg.textBox_password.Text = strPassword;
            dlg.checkBox_savePassword.Checked = true;

            dlg.textBox_serverAddr.Enabled = false;
            dlg.TopMost = true; // 2009/11/12 因为ShowDialog(null)，为了防止对话框被放在非顶部
            dlg.ShowDialog(null);
            if (dlg.DialogResult != DialogResult.OK)
            {
                e.Cancel = true;    // 2009/11/12 如果缺这一句，会造成Cancel后仍然重新弹出登录对话框
                return;
            }

            strUserName = dlg.textBox_userName.Text;
            strPassword = dlg.textBox_password.Text;

            e.UserName = strUserName;
            e.Password = strPassword;

            this.DetailForm.MainForm.ParamTable["author_number_account_username"] = strUserName;
            this.DetailForm.MainForm.ParamTable["author_number_account_password"] = strPassword;
        }

        void DoGcatStop(object sender, StopEventArgs e)
        {
            if (this.GcatChannel != null)
                this.GcatChannel.Abort();
        }

        bool bMarcEditorFocued = false;

        public void BeginGcatLoop(string strMessage)
        {
            bMarcEditorFocued = this.DetailForm.MarcEditor.Focused;
            this.DetailForm.EnableControls(false);

            Stop stop = this.DetailForm.stop;

            stop.OnStop += new StopEventHandler(this.DoGcatStop);
            stop.Initial(strMessage);
            stop.BeginLoop();

            this.DetailForm.Update();
            this.DetailForm.MainForm.Update();
        }

        public void EndGcatLoop()
        {
            Stop stop = this.DetailForm.stop;
            stop.EndLoop();
            stop.OnStop -= new StopEventHandler(this.DoGcatStop);
            stop.Initial("");

            this.DetailForm.EnableControls(true);
            if (bMarcEditorFocued == true)
                this.DetailForm.MarcEditor.Focus();
        }

        // 过滤出包含汉字的字符串
        public List<string> ContainHanzi(List<string> authors)
        {
            List<string> results = new List<string>();
            foreach (string strAuthor in authors)
            {
                if (ContainHanzi(strAuthor) == true)
                    results.Add(strAuthor);
            }

            return results;
        }

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

        // 获得著者号 -- 四角号码
        // return:
        //      -1  error
        //      0   canceled
        //      1   succeed
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
            nRet = this.DetailForm.MainForm.HanziTextToSjhm(
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

        // 数据保存前的处理工作
        public virtual void BeforeSaveRecord(object sender,
            BeforeSaveRecordEventArgs e)
        {
            if (sender == null)
                return;

            int nRet = 0;
            string strError = "";
            bool bChanged = false;

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
                    nRet = MacroUtil.GetFromLocalMacroTable(PathUtil.MergePath(this.DetailForm.MainForm.DataDir, "marceditor_macrotable.xml"),
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
                    strCreator = this.DetailForm.CurrentUserName;
                    if (string.IsNullOrEmpty(strCreator) == true)
                        strCreator = e.CurrentUserName;
                    this.SetFirstSubfield("998", "z", strCreator);
                    bChanged = true;
                }

                e.Changed = bChanged;
            }
        }

        // 2011/8/9
        public string GetFirstSubfield(string strFieldName,
            string strSubfieldName,
            string strIndicatorMatch = "**")
        {
            return this.DetailForm.MarcEditor.Record.Fields.GetFirstSubfield(
                    strFieldName,
                    strSubfieldName,
                    strIndicatorMatch);
        }

        public void SetFirstSubfield(string strFieldName,
            string strSubfieldName,
            string strSubfieldValue)
        {
            this.DetailForm.MarcEditor.Record.Fields.SetFirstSubfield(
                    strFieldName,
                    strSubfieldName,
                    strSubfieldValue);
        }

        // 2011/8/10
        public List<string> GetSubfields(string strFieldName,
            string strSubfieldName,
            string strIndicatorMatch = "**")
        {
            return this.DetailForm.MarcEditor.Record.Fields.GetSubfields(
                    strFieldName,
                    strSubfieldName,
                    strIndicatorMatch);
        }
    }

    public class HostEventArgs : EventArgs
    {
        /*
        // 从何处启动? MarcEditor EntityEditForm
        public object StartFrom = null;
         * */

        // 创建数据的事件参数
        public GenerateDataEventArgs e = null;
    }

    public class PinyinCfgItem
    {
        public string FieldName = "";
        public string IndicatorMatchCase = "";
        public string From = "";
        public string To = "";

        public PinyinCfgItem(XmlNode nodeItem)
        {
            this.FieldName = DomUtil.GetAttr(nodeItem, "name");
            this.IndicatorMatchCase = DomUtil.GetAttr(nodeItem, "indicator");
            this.From = DomUtil.GetAttr(nodeItem, "from");
            this.To = DomUtil.GetAttr(nodeItem, "to");
        }
    }
}
