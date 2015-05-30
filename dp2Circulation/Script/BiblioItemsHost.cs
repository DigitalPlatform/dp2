#define SHITOUTANG  // 石头汤分类法和著者号的支持

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

using DigitalPlatform.Marc;
using DigitalPlatform.Script;
using DigitalPlatform.Text;
using System.Windows.Forms;
using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.GcatClient;
using System.Collections;
using DigitalPlatform.CirculationClient;

namespace dp2Circulation
{
    /// <summary>
    /// 种册 Host 类
    /// 对 MARC 记录的访问尽量用 MarcQuery 的 MarcRecord 实现，以增强适应性
    /// </summary>
    public class BiblioItemsHost : IDetailHost
    {
        ScriptActionCollection _scriptActions = new ScriptActionCollection();
        IBiblioItemsWindow _detailWindow = null;

        #region IDetailHost 接口要求

        public Form Form
        {
            get;
            set;
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

            if (sender is EntityEditForm
                || (sender is BiblioAndEntities && e.FocusedControl is EntityEditControl)
                || sender is EntityControl
                || sender is BindingForm)
            {
                // 创建索取号
                actions.NewItem("创建索取号", "为册记录创建索取号", "CreateCallNumber", false);

                // 管理索取号
                actions.NewItem("管理索取号", "为册记录管理索取号", "ManageCallNumber", false);
            }

            this.ScriptActions = actions;
        }

#if NO
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
#endif

        #endregion



        /// <summary>
        /// GCAT 通讯通道
        /// </summary>
        DigitalPlatform.GcatClient.Channel GcatChannel = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public BiblioItemsHost()
        {

        }

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

            // 不对界面进行处理，只是返回结果字符串
            if (sender is BiblioAndEntities && e.Parameter is GetCallNumberParameter)
            {
                BiblioAndEntities biblio = sender as BiblioAndEntities;
                List<CallNumberItem> callnumber_items = biblio.GetCallNumberItems();
                GetCallNumberParameter parameter = e.Parameter as GetCallNumberParameter;

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
            }

            if (sender is BiblioAndEntities)
            {
                BiblioAndEntities biblio = sender as BiblioAndEntities;
                List<CallNumberItem> callnumber_items = biblio.GetCallNumberItems();
                EntityEditControl edit = e.FocusedControl as EntityEditControl;

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
#if NO
                nRet = CreateOneCallNumber(sender,
                    e,
                    0,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
#endif
            }
            else
            {
                strError = "sender 必须是 EntityEditControl 类型(当前为" + sender.GetType().ToString() + ")";
                goto ERROR1;
            }
            return;
        ERROR1:
            e.ErrorInfo = strError;
            if (e.ShowErrorBox == true)
                MessageBox.Show(this.Form, strError);
        }

        // 获得索取号的第一行
        // return:
        //      -1  error
        //      0   canceled
        //      1   succeed
        public virtual int GetCallNumberHeadLine(
            // index,
            ArrangementInfo info,
            out string strHeadLine,
            out string strError)
        {
            strHeadLine = "";
            strError = "";

            return 1;
        }

        public virtual int CreateOneCallNumber(
            IWin32Window owner,
            List<CallNumberItem> callnumber_items,
            string strExistingAccessNo,
            string strLocation,
            string strItemRecPath,
            //int index,
            out string strAccessNo,
    out string strError)
        {
            strError = "";
            strAccessNo = "";
            int nRet = 0;

            string strClass = "";

            //BindingForm binding = null;
            //EntityEditForm edit = null;
            //EntityControl control = null;
            //BookItem book_item = null;

            ArrangementInfo info = null;

            {

                // 取得馆藏地点
                strLocation = StringUtil.GetPureLocationString(strLocation);

                // 获得关于一个特定馆藏地点的索取号配置信息
                nRet = this._detailWindow.MainForm.GetArrangementInfo(strLocation,
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
                    strExistingAccessNo);

            }

            string strHeadLine = null;

            if (info.CallNumberStyle == "馆藏代码+索取类号+区分号"
                || info.CallNumberStyle == "三行")
            {
                // TODO: 需要一个好的虚函数

                // 获得索取号的第一行
                // return:
                //      -1  error
                //      0   canceled
                //      1   succeed
                nRet = GetCallNumberHeadLine(
                    // index,
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

            string strQufenhao = "";

            if (info.QufenhaoType == "zhongcihao"
                || info.QufenhaoType == "种次号")
            {
                // 获得种次号
                CallNumberForm dlg = new CallNumberForm();

                try
                {
                    dlg.MainForm = this._detailWindow.MainForm;
                    // dlg.TopMost = true;
                    //    dlg.Owner = owner;
                    dlg.MyselfItemRecPath = strItemRecPath;
                    dlg.MyselfParentRecPath = this._detailWindow.BiblioRecPath;
                    dlg.MyselfCallNumberItems = callnumber_items;   // 2009/6/4 new add

                    dlg.Show(owner);

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
                    strError = ex.Message;
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
            strAccessNo =
                (strHeadLine != null ? strHeadLine + "/" : "")
                + strClass +
                (string.IsNullOrEmpty(strQufenhao) == false ?
                "/" + strQufenhao : "");
            return 1;
        ERROR1:
            return -1;
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

            MarcRecord record = new MarcRecord(this._detailWindow.GetMarc());
            strClass = record.select("field[@name='" + strFieldName + "']/subfield[@name='" + strSubfieldName + "']").FirstContent;

            if (String.IsNullOrEmpty(strClass) == true)
            {
                strError = "MARC记录中 " + strFieldName + "$" + strSubfieldName + " 没有找到，因此无法获得索取类号";
                return 0;
            }

            return 1;
        }

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

            string strMarcSyntax = this._detailWindow.MarcSyntax;

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
                else if (strClassType == "其它")
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
                else if (strClassType == "其它")
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

                if (type == "GCAT")
                {
                    // 获得著者号
                    string strGcatWebServiceUrl = this._detailWindow.MainForm.GcatServerUrl;   // "http://dp2003.com/dp2libraryws/gcat.asmx";

                    // 获得著者号
                    // return:
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
            return 0;
        ERROR1:
            return -1;
        }

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

            string strMarcSyntax = this._detailWindow.MarcSyntax;

            if (strQufenhaoType == "GCAT")
            {
                if (strMarcSyntax == "unimarc")
                {
                    List<string> results = null;
                    // 700、710、720
                    results = GetSubfields("field[@name='700' and @indicator1!='A']/subfield[@name='a']");    // "700", "a", "@[^A]."
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }
                    results = GetSubfields("field[@name='701']/subfield[@name='a']"); // "710", "a"
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }
                    results = GetSubfields("field[@name='720']/subfield[@name='a']");   // "720", "a"
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    // 701/711/702/712
                    results = GetSubfields("field[@name='701' and @indicator1!='A']/subfield[@name='a']");   // "701", "a", "@[^A]."
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    results = GetSubfields("field[@name='711']/subfield[@name='a']");   // "711", "a"
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    results = GetSubfields("field[@name='702' and @indicator1!='A']/subfield[@name='a']");   // "702", "a", "@[^A]."
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    results = GetSubfields("field[@name='712']/subfield[@name='a']");   // "712", "a"
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    results = GetSubfields("field[@name='200']/subfield[@name='a']");   // "200", "a"
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        fLevel = 1.1F;
                        goto FOUND;
                    }

                    strError = "MARC记录中 700/710/720/701/711/702/712中均未发现包含汉字的 $a 子字段内容，无法获得著者字符串";
                    return 0;
                FOUND:
                    Debug.Assert(results.Count > 0, "");
                    strAuthor = results[0];
                }
                else if (strMarcSyntax == "usmarc")
                {
#if NO
                    List<string> locations = new List<string>();
                    locations.Add("100a");
                    locations.Add("110a");
                    locations.Add("111a");
                    locations.Add("700a");
                    locations.Add("710a");
                    locations.Add("711a");
#endif
                    strAuthor = GetFirstSubfield("field[@name='100' or @name='110' or @name='111' or @name='700' or @name='710' or @name='711']/subfield[@name='a']");
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

                strError = "MARC记录中 701/711/702/712中均未发现 $a 无法获得著者字符串";
                fLevel = 0;
                return 0;   // not found
            }
            else if (strQufenhaoType == "四角号码")
            {
                if (strMarcSyntax == "unimarc")
                {
                    List<string> results = null;
                    // 700、710、720
                    results = GetSubfields("field[@name='700' and @indicator1!='A']/subfield[@name='a']");    // "700", "a", "@[^A]."
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }
                    results = GetSubfields("field[@name='701']/subfield[@name='a']"); // "710", "a"
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }
                    results = GetSubfields("field[@name='720']/subfield[@name='a']");   // "720", "a"
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    // 701/711/702/712
                    results = GetSubfields("field[@name='701' and @indicator1!='A']/subfield[@name='a']");   // "701", "a", "@[^A]."
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    results = GetSubfields("field[@name='711']/subfield[@name='a']");   // "711", "a"
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    results = GetSubfields("field[@name='702' and @indicator1!='A']/subfield[@name='a']");   // "702", "a", "@[^A]."
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    results = GetSubfields("field[@name='712']/subfield[@name='a']");   // "712", "a"
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    results = GetSubfields("field[@name='200']/subfield[@name='a']");   // "200", "a"
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        fLevel = 1.1F;
                        goto FOUND;
                    }

                    strError = "MARC记录中 700/710/720/701/711/702/712中均未发现包含汉字的 $a 子字段内容，无法获得著者字符串";
                    fLevel = 0;
                    return 0;
                FOUND:
                    Debug.Assert(results.Count > 0, "");
                    strAuthor = results[0];
                }
                else if (strMarcSyntax == "usmarc")
                {
#if NO
                    List<string> locations = new List<string>();
                    locations.Add("100a");
                    locations.Add("110a");
                    locations.Add("111a");
                    locations.Add("700a");
                    locations.Add("710a");
                    locations.Add("711a");
#endif
                    strAuthor = GetFirstSubfield("field[@name='100' or @name='110' or @name='111' or @name='700' or @name='710' or @name='711']/subfield[@name='a']");
                }
                else
                {
                    strError = "未知的MARC格式 '" + strMarcSyntax + "'";
                    return -1;
                }

                if (String.IsNullOrEmpty(strAuthor) == false)
                    return 1;   // found

                strError = "MARC记录中 701/711/702/712中均未发现 $a 无法获得著者字符串";
                fLevel = 0;
                return 0;   // not found
            }
            else if (strQufenhaoType == "Cutter-Sanborn Three-Figure")
            {
                if (strMarcSyntax == "unimarc")
                {
                    List<string> results = null;
                    // 700、710、720
                    results = GetSubfields("field[@name='700' and @indicator1!='A']/subfield[@name='a']");    // "700", "a", "@[^A]."
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }
                    results = GetSubfields("field[@name='701']/subfield[@name='a']"); // "710", "a"
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }
                    results = GetSubfields("field[@name='720']/subfield[@name='a']");   // "720", "a"
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    // 701/711/702/712
                    results = GetSubfields("field[@name='701' and @indicator1!='A']/subfield[@name='a']");   // "701", "a", "@[^A]."
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    results = GetSubfields("field[@name='711']/subfield[@name='a']");   // "711", "a"
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    results = GetSubfields("field[@name='702' and @indicator1!='A']/subfield[@name='a']");   // "702", "a", "@[^A]."
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    results = GetSubfields("field[@name='712']/subfield[@name='a']");   // "712", "a"
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        goto FOUND;
                    }

                    results = GetSubfields("field[@name='200']/subfield[@name='a']");   // "200", "a"
                    results = ContainHanzi(results);
                    if (results.Count > 0)
                    {
                        fLevel = 1.0F;   // unimarc 格式中找到题名字符串，英文，作为卡特表用途，要弱一些
                        goto FOUND;
                    }

                    strError = "MARC记录中 700/710/720/701/711/702/712中均未发现不含汉字的 $a 子字段内容，无法获得西文著者字符串";
                    fLevel = 0;
                    return 0;
                FOUND:
                    Debug.Assert(results.Count > 0, "");
                    strAuthor = results[0];
                }
                else if (strMarcSyntax == "usmarc")
                {
#if NO
                    List<string> locations = new List<string>();
                    locations.Add("100a");
                    locations.Add("110a");
                    locations.Add("111a");
                    locations.Add("700a");
                    locations.Add("710a");
                    locations.Add("711a");
#endif
                    strAuthor = GetFirstSubfield("field[@name='100' or @name='110' or @name='111' or @name='700' or @name='710' or @name='711']/subfield[@name='a']");
                }
                else
                {
                    strError = "未知的MARC格式 '" + strMarcSyntax + "'";
                    return -1;
                }

                if (String.IsNullOrEmpty(strAuthor) == false)
                    return 1;   // found

                // TODO: 245$a 中找到的英文的题名字符串，要强一些，level 1.1

                strError = "MARC记录中无法获得著者字符串";
                fLevel = 0;
                return 0;   // not found
            }
#if SHITOUTANG
            else if (strQufenhaoType == "石头汤著者号"
                || strQufenhaoType == "石头汤")
            {
                MarcRecord record = new MarcRecord(this._detailWindow.GetMarc());

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
            if (string.IsNullOrEmpty(this.DetailForm.MainForm.PinyinServerUrl) == true
               || this.DetailForm.MainForm.ForceUseLocalPinyinFunc == true)
            {
                nRet = this.DetailForm.MainForm.HanziTextToPinyin(
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
                nRet = this.DetailForm.MainForm.SmartHanziTextToPinyin(
                    this.DetailForm,
                    strText,
                    PinyinStyle.None,
                    false,  // auto sel
                    out strPinyin,
                    out strError);
            }
#endif
            nRet = this._detailWindow.MainForm.GetPinyin(
                this._detailWindow.Form,
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

        #region GCAT

        void DoGcatStop(object sender, StopEventArgs e)
        {
            if (this.GcatChannel != null)
                this.GcatChannel.Abort();
        }

        /// <summary>
        /// 开始进行 GCAT 通讯操作
        /// </summary>
        /// <param name="strMessage">要在状态行显示的提示信息</param>
        public void BeginGcatLoop(string strMessage)
        {
            //bMarcEditorFocued = this.DetailForm.MarcEditor.Focused;
            //this.DetailForm.EnableControls(false);

            Stop stop = this._detailWindow.Progress;

            stop.OnStop += new StopEventHandler(this.DoGcatStop);
            stop.Initial(strMessage);
            stop.BeginLoop();

            //this.DetailForm.Update();
            //this.DetailForm.MainForm.Update();
        }

        /// <summary>
        /// 结束 GCAT 通讯操作
        /// </summary>
        public void EndGcatLoop()
        {
            Stop stop = this._detailWindow.Progress;
            stop.EndLoop();
            stop.OnStop -= new StopEventHandler(this.DoGcatStop);
            stop.Initial("");

            //this.DetailForm.EnableControls(true);
            //if (bMarcEditorFocued == true)
            //    this.DetailForm.MarcEditor.Focus();
        }


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
                        this._detailWindow.Progress,
                        this._detailWindow.Form,
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

                string strID = this._detailWindow.MainForm.AppInfo.GetString("DetailHost", "gcat_id", "");
                bool bSaveID = this._detailWindow.MainForm.AppInfo.GetBoolean("DetailHost", "gcat_saveid", false);

                Hashtable question_table = (Hashtable)this._detailWindow.MainForm.ParamTable["question_table"];
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
                        this._detailWindow.Progress,
                        this._detailWindow.Form,
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
                        GuiUtil.SetControlFont(login_dlg, 
                            this._detailWindow.MainForm.DefaultFont, 
                            false);
                        login_dlg.Text = "获得著者号 -- "
                            + ((string.IsNullOrEmpty(strID) == true) ? "请输入ID" : strError);
                        login_dlg.ID = strID;
                        login_dlg.SaveID = bSaveID;
                        login_dlg.StartPosition = FormStartPosition.CenterScreen;
                        if (login_dlg.ShowDialog(this._detailWindow.Form) == DialogResult.Cancel)
                        {
                            return -1;
                        }

                        strID = login_dlg.ID;
                        bSaveID = login_dlg.SaveID;
                        if (login_dlg.SaveID == true)
                        {
                            this._detailWindow.MainForm.AppInfo.SetString("DetailHost", "gcat_id", strID);
                        }
                        else
                        {
                            this._detailWindow.MainForm.AppInfo.SetString("DetailHost", "gcat_id", "");
                        }
                        this._detailWindow.MainForm.AppInfo.SetBoolean("DetailHost", "gcat_saveid", bSaveID);
                        goto REDO_GETNUMBER;
                    }

                    this._detailWindow.MainForm.ParamTable["question_table"] = question_table;

                    return nRet;
                }
                finally
                {
                    EndGcatLoop();
                }
            }
        }

        internal void gcat_channel_BeforeLogin(object sender,
    DigitalPlatform.GcatClient.BeforeLoginEventArgs e)
        {
            string strUserName = (string)this._detailWindow.MainForm.ParamTable["author_number_account_username"];
            string strPassword = (string)this._detailWindow.MainForm.ParamTable["author_number_account_password"];

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
            GuiUtil.SetControlFont(dlg, this._detailWindow.MainForm.DefaultFont, false);

            if (e.Failed == true)
                dlg.textBox_comment.Text = "登录失败。加著者号码功能需要重新登录";
            else
                dlg.textBox_comment.Text = "加著者号码功能需要登录";

            dlg.textBox_serverAddr.Text = e.GcatServerUrl;
            dlg.textBox_userName.Text = strUserName;
            dlg.textBox_password.Text = strPassword;
            dlg.checkBox_savePassword.Checked = true;

            dlg.textBox_serverAddr.Enabled = false;
            dlg.TopMost = true; // 2009/11/12 new add 因为ShowDialog(null)，为了防止对话框被放在非顶部
            dlg.ShowDialog(null);
            if (dlg.DialogResult != DialogResult.OK)
            {
                e.Cancel = true;    // 2009/11/12 new add 如果缺这一句，会造成Cancel后仍然重新弹出登录对话框
                return;
            }

            strUserName = dlg.textBox_userName.Text;
            strPassword = dlg.textBox_password.Text;

            e.UserName = strUserName;
            e.Password = strPassword;

            this._detailWindow.MainForm.ParamTable["author_number_account_username"] = strUserName;
            this._detailWindow.MainForm.ParamTable["author_number_account_password"] = strPassword;
        }

#endregion

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
            nRet = this._detailWindow.MainForm.HanziTextToSjhm(
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

            int nRet = this._detailWindow.MainForm.LoadQuickCutter(true, out strError);
            if (nRet == -1)
                return -1;

            string strText = "";
            string strNumber = "";
            // return:
            //      -1  error
            //      0   not found
            //      1   found
            nRet = this._detailWindow.MainForm.QuickCutter.GetEntry(strAuthor,
                out strText,
                out strNumber,
                out strError);
            if (nRet == -1 || nRet == 0)
                return -1;

            strAuthorNumber = strText[0] + strNumber;
            return 1;
        }

        // 获得若干子字段内容
        public List<string> GetSubfields(string strXPath)
        {
            List<string> results = new List<string>();
            MarcRecord record = new MarcRecord(this._detailWindow.GetMarc());
            MarcNodeList nodes = record.select(strXPath);
            foreach(MarcSubfield subfield in nodes)
            {
                if (string.IsNullOrEmpty(subfield.Content) == false)
                    results.Add(subfield.Content);
            }

            return results;
        }

        // 得到第一个非空的子字段内容
        public string GetFirstSubfield(string strXPath)
        {
            MarcRecord record = new MarcRecord(this._detailWindow.GetMarc());
            MarcNodeList nodes = record.select(strXPath);
            foreach (MarcSubfield subfield in nodes)
            {
                if (string.IsNullOrEmpty(subfield.Content) == false)
                    return subfield.Content;
            }

            return null;
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


    }

    public class GetCallNumberParameter
    {
        public string ExistingAccessNo = "";    // [in]
        public string Location = "";
        public string RecPath = "";

        public string ResultAccessNo = "";  // [out]
    }
}
