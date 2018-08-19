// dp2circulation 西文图书 USMARC 自动创建数据C#脚本程序
// 最后修改时间 2015/1/3

// 1) 2011/8/18 加入GetTemplateDef()回调函数
// 2) 2015/1/3 增加加入封面功能

using System;
using System.Collections;
using System.Collections.Generic;	// List<?>
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Diagnostics;	// Debug.

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.Marc;
using DigitalPlatform.MarcDom;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Script;
using DigitalPlatform.CommonControl;	// Field856Dialog
using DigitalPlatform.AmazonInterface;

using dp2Circulation;

public class MyHost : DetailHost
{
    public void CreateMenu(object sender, GenerateDataEventArgs e)
    {
        ScriptActionCollection actions = new ScriptActionCollection();

        if (sender is MarcEditor || sender == null)
        {
            // 规整ISBN为13
            actions.NewItem("规整ISBN-13", "对020$a中ISBN进行规整", "HyphenISBN_13", false);

            // 规整ISBN为10
            actions.NewItem("规整ISBN-10", "对020$a中ISBN进行规整", "HyphenISBN_10", false);

            //245$c<-- 100$a
            actions.NewItem("245$c<-- 100$a", "将100$a内容拷贝入245字段$c", "Copy100aTo245c", false);

            //7xx$a<-- 245$c
            actions.NewItem("7xx$a<-- 245$c", "将245$c内容拷贝入7xx字段$a", "Copy245cTo7xxa", false);

            // 出版地
            actions.NewItem("260$a$b <-- 020$a", "根据020$a中ISBN出版社代码, 自动创建出版社子字段260$a$b", "AddPublisher", false);

            // 维护 260 出版地 出版社
            actions.NewItem("维护260对照表", "ISBN出版社代码 和 260字段$a出版地$b出版社名 的对照表", "Manage260", false);
            
            // 分割行
            actions.NewSeperator();

            actions.NewItem("加入封面图片 URL", "将来自亚马逊的封面图片 URL 加入 856 字段", "AddCoverImageUrl", false);

            // 分割行
            actions.NewSeperator();
        }

        if (sender is BinaryResControl || sender is MarcEditor)
        {
            // 856字段
            actions.NewItem("创建维护856字段", "创建维护856字段", "Manage856", false);
        }

        if (sender is EntityEditForm || sender is EntityControl)
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

    public override void CreateCallNumber(object sender,
    GenerateDataEventArgs e)
    {
        base.CreateCallNumber(sender, e);
    }

    #region 设置菜单加亮状态

    // 设置菜单加亮状态 -- 加入封面图像 URL
    void AddCoverImageUrl_setMenu(object sender, SetMenuEventArgs e)
    {
        Field curfield = this.DetailForm.MarcEditor.FocusedField;
        if (curfield == null || curfield.Name != "020")
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

    // 设置菜单加亮状态 -- 规整ISBN为13
    void HyphenISBN_13_setMenu(object sender, SetMenuEventArgs e)
    {
        Field curfield = this.DetailForm.MarcEditor.FocusedField;
        if (curfield == null || curfield.Name != "020")
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
        if (curfield == null || curfield.Name != "020")
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

    // 设置菜单加亮状态 -- 245$c<-- 100$a
    void Copy100aTo245c_setMenu(object sender, SetMenuEventArgs e)
    {
        Field curfield = this.DetailForm.MarcEditor.FocusedField;
        if (curfield != null
            && curfield.Name == "245" && this.DetailForm.MarcEditor.FocusedSubfieldName == 'c')
            e.Action.Active = true;
        else
            e.Action.Active = false;
    }

    // 设置菜单加亮状态 -- 7xx$a<-- 245$c
    void Copy245cTo7xxa_setMenu(object sender, SetMenuEventArgs e)
    {
        Field curfield = this.DetailForm.MarcEditor.FocusedField;
        if (curfield != null &&
                (curfield.Name == "700"
                || curfield.Name == "710"
                || curfield.Name == "711"))
            e.Action.Active = true;
        else
            e.Action.Active = false;
    }

    // 设置菜单加亮状态 -- 出版地
    void AddPublisher_setMenu(object sender, SetMenuEventArgs e)
    {
        Field curfield = this.DetailForm.MarcEditor.FocusedField;
        if (curfield != null && curfield.Name == "260")
            e.Action.Active = true;
        else
            e.Action.Active = false;
    }
    #endregion

    // 加入出版地、出版者
    void AddPublisher()
    {
        string strError = "";
        string strISBN = "";

        int nRet = 0;

        strISBN = this.DetailForm.MarcEditor.Record.Fields.GetFirstSubfield("020", "a");

        if (strISBN.Trim() == "")
        {
            strError = "记录中不存在020$a子字段,因此无法加出版社子字段";
            goto ERROR1;
        }



        // 切割出 出版社 代码部分
        string strPublisherNumber = "";
        nRet = this.DetailForm.MainForm.GetPublisherNumber(strISBN,
            out strPublisherNumber,
            out strError);
        if (nRet == -1)
        {
            goto ERROR1;
        }

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

        this.DetailForm.MarcEditor.Record.Fields.SetFirstSubfield("260", "a", strCity + " :");
        this.DetailForm.MarcEditor.Record.Fields.SetFirstSubfield("260", "b", strName + ",");


        return;

    ERROR1:
        MessageBox.Show(this.DetailForm, strError);
    }

    // 维护260对照关系
    void Manage260()
    {
        string strError = "";
        string strISBN = "";
        int nRet = 0;

        strISBN = this.DetailForm.MarcEditor.Record.Fields.GetFirstSubfield("020", "a").Trim();

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
            strPublisherNumber = "978-0-?";

        strPublisherNumber = InputDlg.GetInput(
                this.DetailForm,
                "维护260对照表 -- 第1步",
                "请输入ISBN中出版社号码部分:",
                strPublisherNumber,
                this.DetailForm.MainForm.DefaultFont);
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
            // 获得现有的260字段 $a$b
            Field field_260 = this.DetailForm.MarcEditor.Record.Fields.GetOneField("260", 0);
            if (field_260 != null)
            {
                Subfield subfield_a = field_260.Subfields["a"];
                Subfield subfield_b = field_260.Subfields["b"];
                if (subfield_a != null && subfield_b != null
                    && string.IsNullOrEmpty(subfield_a.Value) == false
                    && string.IsNullOrEmpty(subfield_b.Value) == false)
                {
                    strValue = FilterDocument.TrimEndChar(subfield_a.Value.Trim()).Trim()
                        + ":"
                        + FilterDocument.TrimEndChar(subfield_b.Value.Trim()).Trim();
                }
            }

            if (string.IsNullOrEmpty(strValue) == true)
                strValue = "出版地:出版社名";
        }

        // 创建新条目
        strValue = InputDlg.GetInput(
            this.DetailForm,
            "维护260对照表 -- 第2步",
            "请输入ISBN出版社号码 '" + strPublisherNumber + "' 对应的 MARC21 260$a$c参数(格式 出版地:出版社名):",
            strValue,
                this.DetailForm.MainForm.DefaultFont);
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

        strISBN = this.DetailForm.MarcEditor.Record.Fields.GetFirstSubfield("020", "a");

        if (strISBN.Trim() == "")
        {
            MessageBox.Show(this.DetailForm, "记录中不存在020$a子字段,因此无法进行规整");
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

        this.DetailForm.MarcEditor.Record.Fields.SetFirstSubfield("020", "a", strResult);

        return;

    ERROR1:
        MessageBox.Show(this.DetailForm, strError);

    }

    //100$a-->245$c
    void Copy100aTo245c()
    {
        Copy100aTo245c("a", "245");

    }

    void Copy100aTo245c(string strFromSubfield, string strToField)
    {
        Field field_100 = this.DetailForm.MarcEditor.Record.Fields.GetOneField("100", 0);
        if (field_100 == null)
        {
            MessageBox.Show(this.DetailForm, "100字段不存在");
            return;
        }
        SubfieldCollection subfields_100 = field_100.Subfields;

        Subfield subfield_f = subfields_100[strFromSubfield];

        if (subfield_f == null)
        {
            MessageBox.Show(this.DetailForm, "100$" + strFromSubfield + "不存在");
            return;
        }
        string strContent = subfield_f.Value;

        // 看看当前活动字段是不是245
        Field field_245 = null;

        field_245 = this.DetailForm.MarcEditor.FocusedField;
        if (field_245 != null)
        {
            if (field_245.Name != strToField)
                field_245 = null;
        }

        if (field_245 == null)
        {
            field_245 = this.DetailForm.MarcEditor.Record.Fields.GetOneField(strToField, 0);

            if (field_245 == null)
            {
                field_245 = this.DetailForm.MarcEditor.Record.Fields.Add(strToField, "  ", "", true);
            }

        }

        if (field_245 == null)
            throw (new Exception("error ..."));


        Subfield subfield_245a = field_245.Subfields["c"];
        if (subfield_245a == null)
        {
            subfield_245a = new Subfield();
            subfield_245a.Name = "c";
        }

        subfield_245a.Value = strContent;
        field_245.Subfields["c"] = subfield_245a;

    }

    //245$c-->7xx$a
    void Copy245cTo7xxa()
    {
        Copy245cTo7xxa("c", "700,710,711");
    }
    // 取列表值的第一个
    static string FirstOf(string strParts)
    {
        string[] parts = strParts.Split(new char[] { ',' });
        if (parts.Length > 0)
            return parts[0];

        return strParts;
    }
    void Copy245cTo7xxa(string strFromSubfield, string strToFields)
    {
        Field field_245 = this.DetailForm.MarcEditor.Record.Fields.GetOneField("245", 0);
        if (field_245 == null)
        {
            MessageBox.Show(this.DetailForm, "245字段不存在");
            return;
        }
        SubfieldCollection subfields_245 = field_245.Subfields;

        Subfield subfield_f = subfields_245[strFromSubfield];

        if (subfield_f == null)
        {
            MessageBox.Show(this.DetailForm, "245$" + strFromSubfield + "不存在");
            return;
        }
        string strContent = subfield_f.Value;

        // 看看当前活动字段是不是700
        Field field_700 = null;

        field_700 = this.DetailForm.MarcEditor.FocusedField;
        string strToField = FirstOf(strToFields);
        if (field_700 != null)
        {
            if (StringUtil.IsInList(field_700.Name, strToFields) == true)
                strToField = field_700.Name;
            else
                field_700 = null;
        }


        if (field_700 == null)
        {
            field_700 = this.DetailForm.MarcEditor.Record.Fields.GetOneField(strToField, 0);

            if (field_700 == null)
            {
                field_700 = this.DetailForm.MarcEditor.Record.Fields.Add(strToField, "  ", "", true);
            }

        }


        if (field_700 == null)
            throw (new Exception("error ..."));


        Subfield subfield_700a = field_700.Subfields["a"];
        if (subfield_700a == null)
        {
            subfield_700a = new Subfield();
            subfield_700a.Name = "a";
        }

        subfield_700a.Value = strContent;
        field_700.Subfields["a"] = subfield_700a;

    }

    // 加入封面图像 URL
    void AddCoverImageUrl()
    {
        string strError = "";
        string strISBN = "";
        int nRet = 0;

        MainForm main_form = this.DetailForm.MainForm;

        strISBN = this.DetailForm.MarcEditor.Record.Fields.GetFirstSubfield("020", "a");

        if (string.IsNullOrEmpty(strISBN) == false)
        {
            strISBN = strISBN.Trim();
            int index = strISBN.IndexOf(" ");
            if (index != -1)
                strISBN = strISBN.Substring(0, index).Trim();
        }

        if (string.IsNullOrEmpty(strISBN) == true)
        {
            strError = "记录中不存在 020$a 子字段, 因此无法加入封面图片 URL";
            goto ERROR1;
        }

        nRet = main_form.LoadIsbnSplitter(true, out strError);
        if (nRet == -1)
            goto ERROR1;

        AmazonSearchForm dlg = new AmazonSearchForm();

        string strOutputISBN = "";
        Debug.Assert(main_form != null, "1");
        Debug.Assert(main_form.IsbnSplitter != null, "2");

        nRet = main_form.IsbnSplitter.IsbnInsertHyphen(strISBN, "force10", out strOutputISBN, out strError);
        if (nRet != -1)
        {
            string strHead = "";
            string strOther = "";

            StringUtil.ParseTwoPart(strOutputISBN, "-", out strHead, out strOther);

            if (strHead == "7")
                dlg.ServerUrl = "webservices.amazon.cn";  // 中国
            else
                dlg.ServerUrl = "webservices.amazon.com";  // 美国
        }
        else
            dlg.ServerUrl = "webservices.amazon.cn";  // 中国

        MainForm.SetControlFont(dlg, this.DetailForm.Font, false);
        dlg.TempFileDir = main_form.DataDir;
        dlg.AutoSearch = true;

        dlg.UiState = main_form.AppInfo.GetString(
"TestForm",
"AmazonSearchForm_uiState",
"");
        dlg.QueryWord = strISBN;
        dlg.From = "ISBN";

        main_form.AppInfo.LinkFormState(dlg, "TestForm_AmazonSearchForm_state");

        dlg.ShowDialog(this.DetailForm);

        main_form.AppInfo.UnlinkFormState(dlg);
        main_form.AppInfo.SetString(
"TestForm",
"AmazonSearchForm_uiState",
dlg.UiState);
        if (dlg.DialogResult == DialogResult.Cancel)
            return;

        Hashtable table = dlg.GetImageUrls(dlg.SelectedItem.Xml);
        foreach (string name in table.Keys)
        {
            AmazonSearch.ImageInfo info = table[name] as AmazonSearch.ImageInfo;

            Field field_856 = this.DetailForm.MarcEditor.Record.Fields.Add("856",
                "4 ",
                "",
                true);

            NewSubfield(field_856, "3", "Cover image");
            NewSubfield(field_856, "u", info.Url);
            NewSubfield(field_856, "q", AmazonSearch.GetMime(info.Url));

            NewSubfield(field_856, "x", "type:FrontCover." + name + ";size:" + info.Size + ";source:Amazon:" + dlg.SelectedItem.ASIN);
        }

        return;
    ERROR1:
        MessageBox.Show(this.DetailForm, strError);
    }

    static void NewSubfield(
        Field field,
        string strName,
        string strContent)
    {
        Subfield subfield = new Subfield();
        subfield.Name = strName;
        subfield.Value = strContent;
        field.Subfields[strName] = subfield;
    }


    // 获得模板定义
    void GetTemplateDef(object sender, GetTemplateDefEventArgs e)
    {
        if (e.FieldName == "008")
        {
            if (this.DetailForm.MarcEditor.MarcDefDom == null)
            {
                e.ErrorInfo = "MarcEditor中的MarcDefDom尚未准备好...";
                return;
            }

            if (this.DetailForm.MarcEditor.Record.Fields.Count == 0)
            {
                e.ErrorInfo = "MarcEditor中没有头标区";
                return;
            }

            // 观察头标区
            Field header = this.DetailForm.MarcEditor.Record.Fields[0];
            if (header.Value.Length < 24)
            {
                e.ErrorInfo = "MarcEditor中头标区不是24字符";
                return;
            }

            string strType = "";
            // http://www.loc.gov/marc/bibliographic/bd008b.html
            // Books definition of field 008/18-34 is used when Leader/06 (Type of record) contains code a (Language material) or t (Manuscript language material) and Leader/07 (Bibliographic level) contains code a (Monographic component part), c (Collection), d (Subunit), or m (Monograph). 
            if ("at".IndexOf(header.Value[6])!= -1
                && "acdm".IndexOf(header.Value[7]) != -1)
                strType = "books";
            // http://www.loc.gov/marc/bibliographic/bd008c.html
            // Computer files definition of field 008/18-34 is used when Leader/06 (Type of record) contains code m.
            else if ("m".IndexOf(header.Value[6]) != -1)
                strType = "computer_files";
            // http://www.loc.gov/marc/bibliographic/bd008p.html
            // Maps definition of field 008/18-34 is used when Leader/06 (Type of record) contains code e (Cartographic material) or f (Manuscript cartographic material).
            else if ("ef".IndexOf(header.Value[6]) != -1)
                strType = "maps";
            // http://www.loc.gov/marc/bibliographic/bd008m.html
            // Music definition of field 008/18-34 is used when Leader/06 (Type of record) contains code c (Notated music), d (Manuscript notated music), i (Nonmusical sound recording), or j (Musical sound recording).
            else if ("cdij".IndexOf(header.Value[6]) != -1)
                strType = "music";
            // http://www.loc.gov/marc/bibliographic/bd008s.html
            // Continuing resources field 008/18-34 contains coded data for all continuing resources, including serials and integrating resources. It is used when Leader/06 (Type of record) contains code a (Language material) and Leader/07 contains code b (Serial component part), i (Integrating resource), or code s (Serial).
            else if ("a".IndexOf(header.Value[6]) != -1
    && "bis".IndexOf(header.Value[7]) != -1)
                strType = "contining_resources";
            // http://www.loc.gov/marc/bibliographic/bd008v.html
            // Visual materials definition of field 008/18-34 is used when Leader/06 (Type of record) contains code g (Projected medium), code k (Two-dimensional nonprojectable graphic, code o (Kit), or code r (Three-dimensional artifact or naturally occurring object).
            else if ("gkor".IndexOf(header.Value[6]) != -1)
                strType = "visual_materials";
            // http://www.loc.gov/marc/bibliographic/bd008x.html
            // Mixed materials definition of field 008/18-34 is used when Leader/06 (Type of record) contains code p (Mixed material). 
            else if ("p".IndexOf(header.Value[6]) != -1)
                strType = "mixed_materials";
            else 
            {
                e.ErrorInfo = "无法根据当前头标区 '"+header.Value.Replace(" ", "_")+"' 内容辨别文献类型，所以无法获得模板定义";
                return;
            }


            e.DefNode = this.DetailForm.MarcEditor.MarcDefDom.DocumentElement.SelectSingleNode("Field[@name='" + e.FieldName + "' and @type='"+strType+"']");
            if (e.DefNode == null)
            {
                e.ErrorInfo = "字段名为 '" + e.FieldName + "' 类型为='" + strType + "' 的模板定义无法在MARC定义文件中找到";
                return;
            }

            e.Title = "008 " + strType;
            return;
        }
        if (e.FieldName == "007")
        {
            if (this.DetailForm.MarcEditor.MarcDefDom == null)
            {
                e.ErrorInfo = "MarcEditor中的MarcDefDom尚未准备好...";
                return;
            }

            string strType = "";

            if (e.Value.Length < 1)
            {
                // 权且当作 'a' 处理
                strType = "map";
            }
            else
            {
                // http://www.loc.gov/marc/bibliographic/bd007.html
                // Map (007/00=a)
                if (e.Value[0] == 'a')
                    strType = "map";
                // Electronic resource (007/00=c)
                else if (e.Value[0] == 'c')
                    strType = "electronic_resource";
                // Globe (007/00=d)
                else if (e.Value[0] == 'd')
                    strType = "globe";
                // Tactile material (007/00=f)
                else if (e.Value[0] == 'f')
                    strType = "tactile_material";
                // Projected graphic (007/00=g)
                else if (e.Value[0] == 'g')
                    strType = "projected_graphic";
                // Microform (007/00=h)
                else if (e.Value[0] == 'h')
                    strType = "microform";
                // Nonprojected graphic (007/00=k)
                else if (e.Value[0] == 'k')
                    strType = "nonprojected_graphic";
                // Motion picture (007/00=m)
                else if (e.Value[0] == 'm')
                    strType = "motion_picture";
                // Kit (007/00=o)
                else if (e.Value[0] == 'o')
                    strType = "kit";
                // Notated music (007/00=q)
                else if (e.Value[0] == 'q')
                    strType = "notated_music";
                // Remote-sensing image (007/00=r)
                else if (e.Value[0] == 'r')
                    strType = "remote-sensing_image";
                // Sound recording (007/00=s)
                else if (e.Value[0] == 's')
                    strType = "sound_recording";
                // Text (007/00=t)
                else if (e.Value[0] == 't')
                    strType = "text";
                // Videorecording (007/00=v)
                else if (e.Value[0] == 'v')
                    strType = "videorecording";
                // Unspecified (007/00=z)
                else if (e.Value[0] == 'z')
                    strType = "unspecified";
                else
                {
                    e.ErrorInfo = "无法根据当前007字段第一字符内容 '" + e.Value[0].ToString() + "' 从MARC定义文件中获得模板定义";
                    return;
                }
            }

            e.DefNode = this.DetailForm.MarcEditor.MarcDefDom.DocumentElement.SelectSingleNode("Field[@name='" + e.FieldName + "' and @type='" + strType + "']");
            if (e.DefNode == null)
            {
                e.ErrorInfo = "字段名为 '" + e.FieldName + "' 类型为='" + strType + "' 的模板定义无法在MARC定义文件中找到";
                return;
            }

            e.Title = "007 " + strType;
            return;
        }

        if (e.FieldName == "006")
        {
            if (this.DetailForm.MarcEditor.MarcDefDom == null)
            {
                e.ErrorInfo = "MarcEditor中的MarcDefDom尚未准备好...";
                return;
            }

            string strType = "";

            if (e.Value.Length < 1)
            {
                // 权且当作 'a' 处理
                strType = "Books";
            }
            else
            {
                // http://www.loc.gov/marc/bibliographic/bd006.html
                // a - Language material
                // Coded data elements relating to nonserial language material.
                if (e.Value[0] == 'a')
                    strType = "Books";
                // c - Notated music
                // Coded data elements relating to notated music.
                else if (e.Value[0] == 'c')
                    strType = "Music";
                // d - Manuscript notated music
                // Coded data elements relating to manuscript notated music.
                else if (e.Value[0] == 'd')
                    strType = "Music";
                // e - Cartographic material
                // Coded data elements relating to nonmanuscript cartographic material.
                else if (e.Value[0] == 'e')
                    strType = "Maps";
                // f - Manuscript cartographic material
                // Coded data elements relating to manuscript cartographic material.
                else if (e.Value[0] == 'f')
                    strType = "Maps";
                // g - Projected medium
                // Coded data elements relating to a projected medium.
                else if (e.Value[0] == 'g')
                    strType = "Visual Materials";
                // i - Nonmusical sound recording
                // Coded data elements relating to a nonmusical sound recording.
                else if (e.Value[0] == 'i')
                    strType = "Music";
                // j - Musical sound recording
                // Coded data elements relating to a musical sound recording.
                else if (e.Value[0] == 'j')
                    strType = "Music";
                // k - Two-dimensional nonprojectable graphic
                // Coded data elements relating to a two-dimensional nonprojectable graphic.
                else if (e.Value[0] == 'k')
                    strType = "Visual Materials";
                // m - Computer file/Electronic resource
                // Coded data elements relating to either a computer file or an electronic resource in form.
                else if (e.Value[0] == 'm')
                    strType = "Computer Files";
                // o - Kit
                // Coded data elements relating to a kit.
                else if (e.Value[0] == 'o')
                    strType = "Visual Materials";
                // p - Mixed material
                // Coded data elements relating to mixed material.
                else if (e.Value[0] == 'p')
                    strType = "Mixed Materials";
                // r - Three-dimensional artifact or naturally occurring object
                // Coded data elements relating to a three-dimensional artifact or naturally occurring object.
                else if (e.Value[0] == 'r')
                    strType = "Visual Materials";
                // s - Serial/Integrating resource
                // Coded data elements relating to the control aspects of a non-printed continuing resource. For serially-controlled printed language material, field 008 is used.
                else if (e.Value[0] == 's')
                    strType = "Continuing Resources";
                // t - Manuscript language material
                // Coded data elements relating to manuscript language material.
                else if (e.Value[0] == 't')
                    strType = "Books";
                else
                {
                    e.ErrorInfo = "无法根据当前006字段第一字符内容 '" + e.Value[0].ToString() + "' 从MARC定义文件中获得模板定义";
                    return;
                }
            }

            e.DefNode = this.DetailForm.MarcEditor.MarcDefDom.DocumentElement.SelectSingleNode("Field[@name='" + e.FieldName + "' and @type='" + strType + "']");
            if (e.DefNode == null)
            {
                e.ErrorInfo = "字段名为 '" + e.FieldName + "' 类型为='" + strType + "' 的模板定义无法在MARC定义文件中找到";
                return;
            }

            e.Title = "006 " + strType;
            return;
        }


        e.Canceled = true;
    }
}