// dp2circulation 中文期刊 MARC 编目自动创建数据C#脚本程序
// DetailHost版本
// 最后修改时间: 2012/9/22

// 修改情况:
// 3) 2011/1/10 AddPinyin()函数作了修改，所创建的$9子字段会紧贴着汉字子字段后面插入
// 5) 2011/6/22 增加调用单张卡片打印功能
// 6) 2011/8/17 采用新的(基类)加拼音函数
// 7) 2011/8/17 删除Main()函数，改用最新的动态菜单方式
// 8) 2012/9/22 CreateMenu()函数增加了对BindingForm的处理
// 9) 2015/12/2 注释掉和 905 字段有关的几个菜单项，以避免用户发生误会

// #define TESTING
using System;
using System.Collections.Generic;	// List<?>
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Diagnostics;	// Debug.

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.Marc;
using DigitalPlatform.IO;
using DigitalPlatform.GcatClient;
using DigitalPlatform.Text;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Script;
using DigitalPlatform.CommonControl;	// Field856Dialog

using dp2Circulation;

public class MyHost : DetailHost
{
    // 新的加拼音字段配置。$9
    string PinyinCfgXml = "<root>"
    + "<item name='200' from='a' to='9' />"
    + "<item name='701' indicator='@[^A].' from='a' to='9' />"
    + "<item name='711' from='a' to='9' />"
    + "<item name='702' indicator='@[^A].' from='a' to='9' />"
    + "<item name='712' from='a' to='9' />"
    + "</root>";

    // 老的加拼音配置。$A等
    string OldPinyinCfgXml = "<root>"
        + "<item name='200' from='aefhi' to='AEFHI' />"
        + "<item name='510' from='aei' to='AEI' />"
        + "<item name='512' from='aei' to='AEI' />"
        + "<item name='513' from='aei' to='AEI' />"
        + "<item name='514' from='aei' to='AEI' />"
        + "<item name='515' from='aei' to='AEI' />"
        + "<item name='516' from='aei' to='AEI' />"
        + "<item name='517' from='aei' to='AEI' />"
        + "<item name='520' from='aei' to='AEI' />"
        + "<item name='530' from='a' to='A' />"
        + "<item name='532' from='a' to='A' />"
        + "<item name='540' from='a' to='A' />"
        + "<item name='541' from='aei' to='AEI' />"
        + "<item name='700' from='a' to='A' />"
        + "<item name='701' indicator='@[^A].' from='a' to='A' />"
        + "<item name='711' from='a' to='A' />"
        + "<item name='702' indicator='@[^A].' from='a' to='A' />"
        + "<item name='712' from='a' to='A' />"
        + "<item name='720' from='a' to='A' />"
        + "<item name='721' from='a' to='A' />"
        + "<item name='722' from='a' to='A' />"
        + "</root>";

    public void CreateMenu(object sender, GenerateDataEventArgs e)
    {
        ScriptActionCollection actions = new ScriptActionCollection();

        if (sender is MarcEditor || sender == null)
        {
#if TESTING
            actions.NewItem("调试用", "调试用", "Test", false);
#endif

            // 加拼音
            actions.NewItem("加拼音", "给全部定义的子字段加拼音", "AddPinyin", false, 'P');

            // 删除拼音
            actions.NewItem("删除拼音", "删除全部拼音子字段", "RemovePinyin", false);

            // 清除拼音缓存
            actions.NewItem("清除拼音缓存", "清除存储的以前选择过的汉字和拼音对照关系", "ClearPinyinCache", false);

            // 分割行
            actions.NewSeperator();

            // 规整ISBN为13
            actions.NewItem("规整为ISBN-13", "对010$a中ISBN进行规整", "HyphenISBN_13", false);

            // 规整ISBN为10
            actions.NewItem("规整为ISBN-10", "对010$a中ISBN进行规整", "HyphenISBN_10", false);

            // 分割行
            actions.NewSeperator();


            // 102国家代码 地区代码
            actions.NewItem("102$a$b <-- 010$a", "根据010$a中ISBN出版社代码, 自动创建102字段$a国家代码$b地区代码", "Add102", false);

            // 410 <-- 225
            actions.NewItem("410 <-- 225", "将225$a内容加入410  $1200  $a", "Copy225To410", false);

            // 7*1$a <-- 200$f
            actions.NewItem("7*1$a <-- 200$f", "将200$f内容加入701/711字段$a", "Copy200fTo7x1a", false);

            // 7*2$a <-- 200$g
            actions.NewItem("7*2$a <-- 200$g", "将200$g内容加入702/712字段$a", "Copy200gTo7x2a", false);

#if NO
            // 905$d <-- 690$a
            actions.NewItem("905$d <-- 690$a", "将690$a内容加入905字段$d", "Copy690aTo905d", false);

            // 加入著者号
            actions.NewItem("加入著者号", "根据701/711/702/712$a内容, 创建905$e", "AddAuthorNumber", false);

            // 加入种次号
            actions.NewItem("加入种次号", "根据905$d内容, 创建905$e", "AddZhongcihao", false);

            //  维护种次号
            actions.NewItem("维护种次号", "根据905$d内容中的类号, 出现维护种次号的界面", "ManageZhongcihao", false);
#endif

            // 出版地
            actions.NewItem("210$a$c <-- 010$a", "根据010$a中ISBN出版社代码, 自动创建出版社子字段210$a$c", "AddPublisher", false);

            // 分割行
            actions.NewSeperator();

            // 维护 102 国家代码 地区代码
            actions.NewItem("维护102对照表", "ISBN出版社代码 和 102字段$a国家代码$b地区代码 的对照表", "Manage102", false);

            // 维护 210 出版地 出版社
            actions.NewItem("维护210对照表", "ISBN出版社代码 和 210字段$a出版地$c出版社名 的对照表", "Manage210", false);

            // 分割行
            actions.NewSeperator();

            // 打印单张卡片
            actions.NewItem("打印单张卡片", "打印单张目录卡片", "PrintSingleCard", false);

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

    #region 设置菜单加亮状态

#if TESTING
    void Test_setMenu(object sender, SetMenuEventArgs e)
    {
        Field curfield = this.DetailForm.MarcEditor.FocusedField;
        char c = this.DetailForm.MarcEditor.FocusedSubfieldName;

        e.Action.Comment = "当前字段名 '" +
            (curfield != null ? curfield.Name : "") 
            +"' 子字段名 '"+c.ToString()+"'";
    }
#endif

    // 设置菜单加亮状态 -- 规整ISBN为13
    void HyphenISBN_13_setMenu(object sender, SetMenuEventArgs e)
    {
        Field curfield = this.DetailForm.MarcEditor.FocusedField;
        if (curfield == null || curfield.Name != "010")
        {
            e.Action.Active = false;
            return;
        }
        Subfield a = curfield.Subfields["a"];
        if (a == null)
        {
            e.Action.Active = false;
            return;
        }

        string strISBN = a.Value;
        if (string.IsNullOrEmpty(strISBN) == true)
        {
            e.Action.Active = false;
            return;
        }

        if (IsbnSplitter.IsIsbn13(strISBN) == true)
        {
            e.Action.Active = false;
            return;
        }

        e.Action.Active = true;
    }

    // 设置菜单加亮状态 -- 规整ISBN为10
    void HyphenISBN_10_setMenu(object sender, SetMenuEventArgs e)
    {
        Field curfield = this.DetailForm.MarcEditor.FocusedField;
        if (curfield == null || curfield.Name != "010")
        {
            e.Action.Active = false;
            return;
        }
        Subfield a = curfield.Subfields["a"];
        if (a == null)
        {
            e.Action.Active = false;
            return;
        }

        string strISBN = a.Value;
        if (string.IsNullOrEmpty(strISBN) == true)
        {
            e.Action.Active = false;
            return;
        }

        if (IsbnSplitter.IsIsbn13(strISBN) == true)
        {
            e.Action.Active = true;
            return;
        }

        e.Action.Active = false;
    }

    // 设置菜单加亮状态 -- 102国家代码 地区代码
    void Add102_setMenu(object sender, SetMenuEventArgs e)
    {
        Field curfield = this.DetailForm.MarcEditor.FocusedField;
        if (curfield != null && curfield.Name == "102")
            e.Action.Active = true;
        else
            e.Action.Active = false;
    }

    // 设置菜单加亮状态 -- 410 <-- 225
    void Copy225To410_setMenu(object sender, SetMenuEventArgs e)
    {
        Field curfield = this.DetailForm.MarcEditor.FocusedField;
        if (curfield != null &&
            (curfield.Name == "225"
            || curfield.Name == "410"))
            e.Action.Active = true;
        else
            e.Action.Active = false;
    }

    // 设置菜单加亮状态 -- 7*1$a <-- 200$f
    void Copy200fTo7x1a_setMenu(object sender, SetMenuEventArgs e)
    {
        Field curfield = this.DetailForm.MarcEditor.FocusedField;
        if (curfield != null &&
                    (curfield.Name == "701"
                    || curfield.Name == "711"))
            e.Action.Active = true;
        else
            e.Action.Active = false;
    }

    // 设置菜单加亮状态 -- 7*2$a <-- 200$g
    void Copy200gTo7x2a_setMenu(object sender, SetMenuEventArgs e)
    {
        Field curfield = this.DetailForm.MarcEditor.FocusedField;
        if (curfield != null &&
                    (curfield.Name == "702"
                    || curfield.Name == "712"))
            e.Action.Active = true;
        else
            e.Action.Active = false;
    }

    // 设置菜单加亮状态 -- 905$d <-- 690$a
    void Copy690aTo905d_setMenu(object sender, SetMenuEventArgs e)
    {
        Field curfield = this.DetailForm.MarcEditor.FocusedField;
        if (curfield != null &&
                    (curfield.Name == "905" || curfield.Name == "690"))
            e.Action.Active = true;
        else
            e.Action.Active = false;
    }

    // 设置菜单加亮状态 -- 加入著者号
    void AddAuthorNumber_setMenu(object sender, SetMenuEventArgs e)
    {
        Field curfield = this.DetailForm.MarcEditor.FocusedField;
        if (curfield != null && curfield.Name == "905")
            e.Action.Active = true;
        else
            e.Action.Active = false;
    }

    // 设置菜单加亮状态 -- 加入种次号
    void AddZhongcihao_setMenu(object sender, SetMenuEventArgs e)
    {
        Field curfield = this.DetailForm.MarcEditor.FocusedField;
        if (curfield != null && curfield.Name == "905"
            && this.DetailForm.MarcEditor.FocusedSubfieldName == 'd')
            e.Action.Active = true;
        else
            e.Action.Active = false;
    }

    // 设置菜单加亮状态 -- 出版地
    void AddPublisher_setMenu(object sender, SetMenuEventArgs e)
    {
        Field curfield = this.DetailForm.MarcEditor.FocusedField;
        if (curfield != null && curfield.Name == "210")
            e.Action.Active = true;
        else
            e.Action.Active = false;
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

    void AddPinyin()
    {
        AddPinyin(this.PinyinCfgXml);
    }

    void RemovePinyin()
    {
        RemovePinyin(this.PinyinCfgXml);
        RemovePinyin(this.OldPinyinCfgXml);
    }

    void ClearPinyinCache()
    {
        this.DetailForm.SetSelectedPinyin(null);
    }

    void PrintSingleCard()
    {
        string strError = "";

        BiblioStatisForm form = this.DetailForm.MainForm.EnsureBiblioStatisForm();	// 取当前最顶层的一个BiblioStatisForm，如果存在的话
        form.Activate();
        form.RecPathList = this.DetailForm.BiblioRecPath;
        form.InputStyle = BiblioStatisInputStyle.RecPaths;
        int nRet = form.RunProject("输出卡片",
                        "single",
                        out strError);
        if (nRet == -1)
        {
            if (string.IsNullOrEmpty(strError) == false)
                MessageBox.Show(this.DetailForm, strError);
        }
        else
            form.Close();
    }

    void Copy200fTo7x1a()
    {
        Copy200gfTo7xxa("f", "701,711");
    }

    void Copy200gTo7x2a()
    {
        Copy200gfTo7xxa("g", "702,712");
    }

    // 取列表值的第一个
    static string FirstOf(string strParts)
    {
        string[] parts = strParts.Split(new char[] { ',' });
        if (parts.Length > 0)
            return parts[0];

        return strParts;
    }

    void Copy200gfTo7xxa(string strFromSubfield, string strToFields)
    {
        Field field_200 = this.DetailForm.MarcEditor.Record.Fields.GetOneField("200", 0);
        SubfieldCollection subfields_200 = field_200.Subfields;

        Subfield subfield_f = subfields_200[strFromSubfield];

        if (subfield_f == null)
        {
            MessageBox.Show(this.DetailForm, "200$" + strFromSubfield + "不存在");
            return;
        }

        string strToField = FirstOf(strToFields);

        string strContent = subfield_f.Value;

        // 看看当前活动字段是不是701
        Field field_701 = null;

        field_701 = this.DetailForm.MarcEditor.FocusedField;
        if (field_701 != null)
        {
            if (StringUtil.IsInList(field_701.Name, strToFields) == true)
                strToField = field_701.Name;
            else
                field_701 = null;
        }

        if (field_701 == null)
        {
            field_701 = this.DetailForm.MarcEditor.Record.Fields.GetOneField(strToField, 0);

            if (field_701 == null)
                field_701 = this.DetailForm.MarcEditor.Record.Fields.Add(strToField, "  ", "", true);
        }

        if (field_701 == null)
            throw (new Exception("error ..."));

        Subfield subfield_701a = field_701.Subfields["a"];
        if (subfield_701a == null)
        {
            subfield_701a = new Subfield();
            subfield_701a.Name = "a";
        }

        subfield_701a.Value = strContent;
        field_701.Subfields["a"] = subfield_701a;
    }

    void AddAuthorNumber()
    {
        string strAuthor = "";

        strAuthor = this.DetailForm.MarcEditor.Record.Fields.GetFirstSubfield("701", "a");

        if (strAuthor != "")
            goto BEGIN;

        strAuthor = this.DetailForm.MarcEditor.Record.Fields.GetFirstSubfield("711", "a");

        if (strAuthor != "")
            goto BEGIN;

        strAuthor = this.DetailForm.MarcEditor.Record.Fields.GetFirstSubfield("702", "a");

        if (strAuthor != "")
            goto BEGIN;

        strAuthor = this.DetailForm.MarcEditor.Record.Fields.GetFirstSubfield("712", "a");

        if (strAuthor == "")
        {
            MessageBox.Show(this.DetailForm, "701/711/702/712中均未发现&a,无法处理");
            return;
        }
    BEGIN:
        string strGcatWebServiceUrl = this.DetailForm.MainForm.GcatServerUrl;   // "http://dp2003.com/dp2libraryws/gcat.asmx";

        string strNumber = "";
        string strError = "";

        // 获得著者号
        // return:
        //      -1  error
        //      0   canceled
        //      1   succeed
        int nRet = GetGcatAuthorNumber(strGcatWebServiceUrl,
            strAuthor,
            out strNumber,
            out strError);
        if (nRet == -1)
            goto ERROR1;

        this.DetailForm.MarcEditor.Record.Fields.SetFirstSubfield("905", "e", strNumber);
        return;
    ERROR1:
        MessageBox.Show(this.DetailForm, strError);
    }

    void AddZhongcihao()
    {
        string strError = "";
        ZhongcihaoForm dlg = new ZhongcihaoForm();

        try
        {
            string strClass = "";
            string strNumber = "";
            int nRet = 0;

            strClass = this.DetailForm.MarcEditor.Record.Fields.GetFirstSubfield("905", "d");

            if (strClass == "")
            {
                MessageBox.Show(this.DetailForm, "记录中不存在905$d子字段,因此无法加种次号");
                return;
            }

            string strExistNumber = this.DetailForm.MarcEditor.Record.Fields.GetFirstSubfield("905", "e");

            dlg.MainForm = this.DetailForm.MainForm;
            dlg.TopMost = true;
            dlg.MyselfBiblioRecPath = this.DetailForm.BiblioRecPath;

            dlg.Show();

            // return:
            //      -1  error
            //      0   canceled
            //      1   succeed
            nRet = dlg.GetNumber(
                ZhongcihaoStyle.Seed,
                        strClass,
                        this.DetailForm.BiblioDbName,
                        out strNumber,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.DetailForm.MarcEditor.Record.Fields.SetFirstSubfield("905", "e", strNumber);
            return;
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

    ERROR1:
        MessageBox.Show(this.DetailForm, strError);
    }

    // 维护种次号
    void ManageZhongcihao()
    {
        string strError = "";
        ZhongcihaoForm dlg = new ZhongcihaoForm();

        string strClass = "";
        int nRet = 0;

        strClass = this.DetailForm.MarcEditor.Record.Fields.GetFirstSubfield("905", "d");

        dlg.MdiParent = this.DetailForm.MainForm;
        dlg.MainForm = this.DetailForm.MainForm;
        dlg.TopMost = true;
        dlg.AutoBeginSearch = true;

        dlg.ClassNumber = strClass;
        dlg.BiblioDbName = this.DetailForm.BiblioDbName;

        dlg.Show();
    }

    // 加入出版地、出版者
    void AddPublisher()
    {
        string strError = "";
        string strISBN = "";

        int nRet = 0;

        strISBN = this.DetailForm.MarcEditor.Record.Fields.GetFirstSubfield("010", "a");

        if (strISBN.Trim() == "")
        {
            strError = "记录中不存在010$a子字段,因此无法加出版社子字段";
            goto ERROR1;
        }

        // 切割出 出版社 代码部分
        string strPublisherNumber = "";
        nRet = this.DetailForm.MainForm.GetPublisherNumber(strISBN,
            out strPublisherNumber,
            out strError);
        if (nRet == -1)
            goto ERROR1;

        string strValue = "";

        nRet = this.DetailForm.GetPublisherInfo(strPublisherNumber,
            out strValue,
            out strError);
        if (nRet == -1)
            goto ERROR1;

        if (nRet == 0 || strValue == "")
        {
            // 创建新条目
            strValue = InputDlg.GetInput(
                this.DetailForm,
                null,
                "请输入ISBN出版社号 '" + strPublisherNumber + "' 对应的出版社名称(格式 出版地:出版社名):",
                "出版地:出版社名");
            if (strValue == null)
                return;	// 放弃整个操作

            nRet = this.DetailForm.SetPublisherInfo(strPublisherNumber,
                strValue,
                out strError);
            if (nRet == -1)
                goto ERROR1;
        }

        // MessageBox.Show(this.DetailForm, strValue);

        // 把全角冒号替换为半角的形态
        strValue = strValue.Replace("：", ":");

        string strName = "";
        string strCity = "";
        nRet = strValue.IndexOf(":");
        if (nRet == -1)
        {
            strName = strValue;
        }
        else
        {
            strCity = strValue.Substring(0, nRet);
            strName = strValue.Substring(nRet + 1);
        }

        this.DetailForm.MarcEditor.Record.Fields.SetFirstSubfield("210", "a", strCity);
        this.DetailForm.MarcEditor.Record.Fields.SetFirstSubfield("210", "c", strName);
        return;
    ERROR1:
        MessageBox.Show(this.DetailForm, strError);
    }

    // 维护210对照关系
    // 2008/10/17 new add
    void Manage210()
    {
        string strError = "";
        string strISBN = "";
        int nRet = 0;

        strISBN = this.DetailForm.MarcEditor.Record.Fields.GetFirstSubfield("010", "a").Trim();

        string strPublisherNumber = "";

        if (String.IsNullOrEmpty(strISBN) == false)
        {
            // 切割出 出版社 代码部分
            nRet = this.DetailForm.MainForm.GetPublisherNumber(strISBN,
                out strPublisherNumber,
                out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }
        }

        if (String.IsNullOrEmpty(strPublisherNumber) == true)
            strPublisherNumber = "978-7-?";

        strPublisherNumber = InputDlg.GetInput(
                this.DetailForm,
                "维护210对照表 -- 第1步",
                "请输入ISBN中出版社号码部分:",
                strPublisherNumber);
        if (strPublisherNumber == null)
            return;	// 放弃整个操作

        string strValue = "";

        nRet = this.DetailForm.GetPublisherInfo(strPublisherNumber,
            out strValue,
            out strError);
        if (nRet == -1)
            goto ERROR1;

        if (nRet == 0 || strValue == "")
        {
            strValue = "出版地:出版社名";
        }

        // 创建新条目
        strValue = InputDlg.GetInput(
            this.DetailForm,
            "维护210对照表 -- 第2步",
            "请输入ISBN出版社号码 '" + strPublisherNumber + "' 对应的UNIMARC 210$a$c参数(格式 出版地:出版社名):",
            strValue);
        if (strValue == null)
            return;	// 放弃整个操作

        if (strValue == "")
            goto DOSAVE;

        // MessageBox.Show(this.DetailForm, strValue);

        // 把全角冒号替换为半角的形态
        strValue = strValue.Replace("：", ":");

        string strName = "";
        string strCity = "";
        nRet = strValue.IndexOf(":");
        if (nRet == -1)
        {
            strError = "输入的内容中缺少冒号";
            goto ERROR1;
            // strName = strValue;
        }
        else
        {
            strCity = strValue.Substring(0, nRet);
            strName = strValue.Substring(nRet + 1);
        }

        strValue = strCity + ":" + strName;

    DOSAVE:
        nRet = this.DetailForm.SetPublisherInfo(strPublisherNumber,
            strValue,
            out strError);
        if (nRet == -1)
            goto ERROR1;
        return;
    ERROR1:
        MessageBox.Show(this.DetailForm, strError);
    }

    void HyphenISBN_13()
    {
        HyphenISBN(true);
    }

    void HyphenISBN_10()
    {
        HyphenISBN(false);
    }

    void HyphenISBN(bool bForce13)
    {
        string strError = "";
        string strISBN = "";
        int nRet = 0;

        strISBN = this.DetailForm.MarcEditor.Record.Fields.GetFirstSubfield("010", "a");

        if (strISBN.Trim() == "")
        {
            MessageBox.Show(this.DetailForm, "记录中不存在010$a子字段,因此无法进行规整");
            return;
        }

        nRet = this.DetailForm.MainForm.LoadIsbnSplitter(true, out strError);
        if (nRet == -1)
            goto ERROR1;

        string strResult = "";

        nRet = this.DetailForm.MainForm.IsbnSplitter.IsbnInsertHyphen(strISBN,
            bForce13 == true ? "force13,strict" : "force10,strict",
                    out strResult,
                    out strError);
        if (nRet == -1)
            goto ERROR1;

        if (nRet == 1)
        {
            DialogResult result = MessageBox.Show(this.DetailForm,
                "原ISBN '" + strISBN + "'加工成 '" + strResult + "' 后发现校验位有变化。\r\n\r\n是否接受修改?",
                "规整ISBN",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

        }

        this.DetailForm.MarcEditor.Record.Fields.SetFirstSubfield("010", "a", strResult);
        return;
    ERROR1:
        MessageBox.Show(this.DetailForm, strError);
    }

    void Add102()
    {
        string strError = "";
        string strISBN = "";
        int nRet = 0;

        strISBN = this.DetailForm.MarcEditor.Record.Fields.GetFirstSubfield("010", "a");

        if (strISBN.Trim() == "")
        {
            strError = "记录中不存在010$a子字段,因此无法加102$a$b";
            goto ERROR1;
        }

        // 切割出 出版社 代码部分
        string strPublisherNumber = "";
        nRet = this.DetailForm.MainForm.GetPublisherNumber(strISBN,
            out strPublisherNumber,
            out strError);
        if (nRet == -1)
            goto ERROR1;

        string strValue = "";

        nRet = this.DetailForm.Get102Info(strPublisherNumber,
            out strValue,
            out strError);
        if (nRet == -1)
            goto ERROR1;

        if (nRet == 0 || strValue == "")
        {
            // 创建新条目
            strValue = InputDlg.GetInput(
                this.DetailForm,
                null,
                "请输入ISBN出版社号码 '" + strISBN + "' 对应的UNIMARC 102$a$b参数(格式 国家代码[2位]:城市代码[6位]):",
                "国家代码[2位]:城市代码[6位]");
            if (strValue == null)
                return;	// 放弃整个操作

            nRet = this.DetailForm.Set102Info(strPublisherNumber,
                strValue,
                out strError);
            if (nRet == -1)
                goto ERROR1;
        }

        // MessageBox.Show(this.DetailForm, strValue);

        // 把全角冒号替换为半角的形态
        strValue = strValue.Replace("：", ":");

        string strCountryCode = "";
        string strCityCode = "";
        nRet = strValue.IndexOf(":");
        if (nRet == -1)
        {
            strCountryCode = strValue;

            if (strCountryCode.Length != 2)
            {
                strError = "国家代码 '" + strCountryCode + "' 应当为2字符";
                goto ERROR1;
            }
        }
        else
        {
            strCountryCode = strValue.Substring(0, nRet);
            strCityCode = strValue.Substring(nRet + 1);
            if (strCountryCode.Length != 2)
            {
                strError = "冒号前面的国家代码部分 '" + strCountryCode + "' 应当为2字符";
                goto ERROR1;
            }
            if (strCityCode.Length != 6)
            {
                strError = "冒号后面的城市代码部分 '" + strCityCode + "' 应当为6字符";
                goto ERROR1;
            }
        }

        this.DetailForm.MarcEditor.Record.Fields.SetFirstSubfield("102", "a", strCountryCode);
        this.DetailForm.MarcEditor.Record.Fields.SetFirstSubfield("102", "b", strCityCode);
        return;
    ERROR1:
        MessageBox.Show(this.DetailForm, strError);
    }

    // 维护102对照关系
    void Manage102()
    {
        string strError = "";
        string strISBN = "";
        int nRet = 0;

        strISBN = this.DetailForm.MarcEditor.Record.Fields.GetFirstSubfield("010", "a").Trim();

        string strPublisherNumber = "";

        if (String.IsNullOrEmpty(strISBN) == false)
        {
            // 切割出 出版社 代码部分
            nRet = this.DetailForm.MainForm.GetPublisherNumber(strISBN,
                out strPublisherNumber,
                out strError);
            if (nRet == -1)
                goto ERROR1;
        }

        if (String.IsNullOrEmpty(strPublisherNumber) == true)
            strPublisherNumber = "978-7-?";

        strPublisherNumber = InputDlg.GetInput(
                this.DetailForm,
                "维护102对照表 -- 第1步",
                "请输入ISBN中出版社号码部分:",
                strPublisherNumber);
        if (strPublisherNumber == null)
            return;	// 放弃整个操作

        string strValue = "";

        nRet = this.DetailForm.Get102Info(strPublisherNumber,
            out strValue,
            out strError);
        if (nRet == -1)
            goto ERROR1;

        if (nRet == 0 || strValue == "")
        {
            strValue = "国家代码[2位]:城市代码[6位]";
        }

        // 创建新条目
        strValue = InputDlg.GetInput(
            this.DetailForm,
            "维护102对照表 -- 第2步",
            "请输入ISBN出版社号码 '" + strPublisherNumber + "' 对应的UNIMARC 102$a$b参数(格式国家代码[2位]:城市代码[6位]):",
            strValue);
        if (strValue == null)
            return;	// 放弃整个操作

        if (strValue == "")
            goto DOSAVE;

        // MessageBox.Show(this.DetailForm, strValue);

        // 把全角冒号替换为半角的形态
        strValue = strValue.Replace("：", ":");

        string strCountryCode = "";
        string strCityCode = "";
        nRet = strValue.IndexOf(":");
        if (nRet == -1)
        {
            strCountryCode = strValue;

            if (strCountryCode.Length != 2)
            {
                strError = "国家代码 '" + strCountryCode + "' 应当为2字符";
                goto ERROR1;
            }
        }
        else
        {
            strCountryCode = strValue.Substring(0, nRet);
            strCityCode = strValue.Substring(nRet + 1);
            if (strCountryCode.Length != 2)
            {
                strError = "冒号前面的国家代码部分 '" + strCountryCode + "' 应当为2字符";
                goto ERROR1;
            }
            if (strCityCode.Length != 6)
            {
                strError = "冒号后面的城市代码部分 '" + strCityCode + "' 应当为6字符";
                goto ERROR1;
            }
        }

        strValue = strCountryCode + ":" + strCityCode;

    DOSAVE:
        nRet = this.DetailForm.Set102Info(strPublisherNumber,
            strValue,
            out strError);
        if (nRet == -1)
            goto ERROR1;
        return;
    ERROR1:
        MessageBox.Show(this.DetailForm, strError);
    }

    void Copy225To410()
    {
        Field field_225 = this.DetailForm.MarcEditor.Record.Fields.GetOneField("225", 0);

        if (field_225 == null)
        {
            MessageBox.Show(this.DetailForm, "225字段不存在");
            return;
        }

        SubfieldCollection subfields_225 = field_225.Subfields;

        Subfield subfield_a = subfields_225["a"];

        if (subfield_a == null)
        {
            MessageBox.Show(this.DetailForm, "225$" + "a" + "不存在");
            return;
        }

        string strContent = subfield_a.Value;

        // 看看当前活动字段是不是410
        Field field_410 = null;

        field_410 = this.DetailForm.MarcEditor.FocusedField;
        if (field_410 != null)
        {
            if (field_410.Name != "410")
                field_410 = null;
        }

        bool bInitial410Value = false;	// 410字段的值是否初始化过

        if (field_410 == null)
        {
            field_410 = this.DetailForm.MarcEditor.Record.Fields.GetOneField("410", 0);

            if (field_410 == null)
            {
                field_410 = this.DetailForm.MarcEditor.Record.Fields.Add("410", "  ", new string((char)31, 1) + "1200  " + new string((char)31, 1) + "a", true);
                bInitial410Value = true;
            }
        }


        if (bInitial410Value == false)
        {
            field_410.Value = new string((char)31, 1) + "1200  " + new string((char)31, 1) + "a" + field_410.Value;
        }

        if (field_410 == null)
            throw (new Exception("error ..."));


        Subfield subfield_410a = field_410.Subfields["a"];
        if (subfield_410a == null)
        {
            subfield_410a = new Subfield();
            subfield_410a.Name = "a";
        }

        subfield_410a.Value = strContent;
        field_410.Subfields["a"] = subfield_410a;
    }


    void Copy690aTo905d()
    {
        Copy690aTo905d("a", "905");
    }

    void Copy690aTo905d(string strFromSubfield, string strToField)
    {
        Field field_690 = this.DetailForm.MarcEditor.Record.Fields.GetOneField("690", 0);
        SubfieldCollection subfields_690 = field_690.Subfields;

        Subfield subfield_a = subfields_690[strFromSubfield];

        if (subfield_a == null)
        {
            MessageBox.Show(this.DetailForm, "690$" + strFromSubfield + "不存在");
            return;
        }

        string strContent = subfield_a.Value;

        // 看看当前活动字段是不是905
        Field field_905 = null;

        field_905 = this.DetailForm.MarcEditor.FocusedField;
        if (field_905 != null)
        {
            if (field_905.Name != strToField)
                field_905 = null;
        }

        if (field_905 == null)
        {
            field_905 = this.DetailForm.MarcEditor.Record.Fields.GetOneField(strToField, 0);

            if (field_905 == null)
            {
                field_905 = this.DetailForm.MarcEditor.Record.Fields.Add(strToField, "  ", "", true);
            }
        }


        if (field_905 == null)
            throw (new Exception("error ..."));

        Subfield subfield_905d = field_905.Subfields["d"];
        if (subfield_905d == null)
        {
            subfield_905d = new Subfield();
            subfield_905d.Name = "d";
        }

        subfield_905d.Value = strContent;
        field_905.Subfields["d"] = subfield_905d;
    }

    public override void CreateCallNumber(object sender,
        GenerateDataEventArgs e)
    {
        base.CreateCallNumber(sender, e);
    }
}