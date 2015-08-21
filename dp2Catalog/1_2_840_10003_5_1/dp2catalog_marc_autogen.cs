// dp2catalog MARC 中文图书 编目自动创建数据C#脚本程序
// 最后修改时间 2013/9/17

// 1) 2011/8/21 public override void Main(object sender, HostEventArgs e)修改为GenerateDataEventArgs e
// 2) 2011/8/21 修改AddAuthorNumber()函数，调用基类的GetGcatAuthorNumber()函数
// 3) 2011/8/22 修改AddAuthorNumber()函数，使之具有忽略第一指示符为'A'的7XX字段$a子字段的能力，并会跳过不包含汉字字符的著者字符串继续向后找
// 4) 2011/8/23 将函数名AddAuthorNumber()修改为AddGcatAuthorNumber()
// 5) 2011/8/24 增加AddSjhmAuthorNumber()。这只是适用于在905$e中加入著者号
// 6) 2011/8/29 Copy200gfTo7xxa()和Copy690aTo905d()函数修改，增加了对字段不存在时的判断和警告
// 7) 2012/1/18 AddZhongcihao()中增加设置服务器名的语句
// 8) 2013/9/17 加拼音能根据 MainForm 的 AutoSelPinyin 参数变化效果


using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.Marc;
using DigitalPlatform.IO;
using DigitalPlatform.GcatClient;
using DigitalPlatform.Text;
using DigitalPlatform.Script;

using dp2Catalog;

public class MyHost : MarcDetailHost
{
    // DigitalPlatform.GcatClient.Channel GcatChannel = null;

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


            // 905$d <-- 690$a
            actions.NewItem("905$d <-- 690$a", "将690$a内容加入905字段$d", "Copy690aTo905d", false);


            // 加入GCA著者号
            actions.NewItem("加入GCAT著者号", "根据701/711/702/712$a内容, 创建905$e", "AddGcatAuthorNumber", false);

            // 加入四角号码著者号
            actions.NewItem("加入四角号码著者号", "根据701/711/702/712$a内容, 创建905$e", "AddSjhmAuthorNumber", false);

            // 加入种次号
            actions.NewItem("加入种次号", "根据905$d内容, 创建905$e", "AddZhongcihao", false);

            //  维护种次号
            actions.NewItem("维护种次号", "根据905$d内容中的类号, 出现维护种次号的界面", "ManageZhongcihao", false);

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

        }

        this.ScriptActions = actions;
    }

    #region 设置菜单加亮状态

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

    // 设置菜单加亮状态 -- 加入GCAT著者号
    void AddGcatAuthorNumber_setMenu(object sender, SetMenuEventArgs e)
    {
        Field curfield = this.DetailForm.MarcEditor.FocusedField;
        if (curfield != null && curfield.Name == "905")
            e.Action.Active = true;
        else
            e.Action.Active = false;
    }

    // 设置菜单加亮状态 -- 加入四角号码著者号
    void AddSjhmAuthorNumber_setMenu(object sender, SetMenuEventArgs e)
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

    #endregion

#if OLD // 废止
    public override void Main(object sender, HostEventArgs e)
	{
		Field curfield = this.DetailForm.MarcEditor.FocusedField;

		ScriptActionCollection actions = new ScriptActionCollection();

		bool bActive = false;


		// 加拼音
		actions.NewItem("加拼音", "给.....加拼音", "AddPinyin", false);

		// 7*1$a <-- 200$f
		if (curfield != null &&
			(curfield.Name == "701"
			|| curfield.Name == "711") )
			bActive = true;
		else
			bActive = false;
			
		actions.NewItem("7*1$a <-- 200$f", "将200$f内容加入701/711字段$a", "Copy200fTo7x1a", bActive);

		// 7*2$a <-- 200$g
		if (curfield != null &&
			(curfield.Name == "702"
			|| curfield.Name == "712") )
			bActive = true;
		else
			bActive = false;
		actions.NewItem("7*2$a <-- 200$g", "将200$g内容加入702/712字段$a", "Copy200gTo7x2a", bActive);

		// 410 <-- 225
		if (curfield != null &&
			(curfield.Name == "225"
			|| curfield.Name == "410") )
			bActive = true;
		else
			bActive = false;
		actions.NewItem("410 <-- 225", "将225$a内容加入410  $1200  $a", "Copy225To410", bActive);



		// 加入著者号
		if (curfield != null && curfield.Name == "905")
			bActive = true;
		else
			bActive = false;

		actions.NewItem("加入著者号", "根据701/711/702/712$a内容, 创建905$e", "AddAuthorNumber", bActive);

		// 加入种次号
		if (curfield != null && curfield.Name == "905" && this.DetailForm.MarcEditor.FocusedSubfieldName == 'd')
			bActive = true;
		else
			bActive = false;
		actions.NewItem("加入种次号", "根据905$d内容, 创建905$e", "AddZhongcihao", bActive);

		//  维护种次号
		actions.NewItem("维护种次号", "根据905$d内容中的类号, 出现维护种次号的界面", "ManageZhongcihao", false);

		// 出版地
		if (curfield != null && curfield.Name == "210")
			bActive = true;
		else
			bActive = false;
		actions.NewItem("210$a$c <-- 010$a", "根据010$a中ISBN出版社代码, 自动创建出版社子字段210$a$c", "AddPublisher", bActive);


		// 规整ISBN为13
		if (curfield != null && curfield.Name == "010")
			bActive = true;
		else
			bActive = false;
		actions.NewItem("规整ISBN-13", "对010$a中ISBN进行规整", "HyphenISBN_13", bActive);

		// 规整ISBN为10
		if (curfield != null && curfield.Name == "010")
			bActive = true;
		else
			bActive = false;
		actions.NewItem("规整ISBN-10", "对010$a中ISBN进行规整", "HyphenISBN_10", bActive);

		// 102国家代码 地区代码
		if (curfield != null && curfield.Name == "102")
			bActive = true;
		else
			bActive = false;
		actions.NewItem("102$a$b <-- 010$a", "根据010$a中ISBN出版社代码, 自动创建102字段$a国家代码$b地区代码", "Add102", bActive);


		ScriptActionMenuDlg dlg = new ScriptActionMenuDlg();

		dlg.Actions = actions;
		if ((Control.ModifierKeys & Keys.Alt)== Keys.Alt)
			dlg.AutoRun = false;
		else
			dlg.AutoRun = this.DetailForm.MainForm.AppInfo.GetBoolean("detailform", "gen_auto_run", false);
		// dlg.StartPosition = FormStartPosition.CenterScreen;

		this.DetailForm.MainForm.AppInfo.LinkFormState(dlg, "gen_data_dlg_state");
		dlg.ShowDialog();
		this.DetailForm.MainForm.AppInfo.UnlinkFormState(dlg);


		this.DetailForm.MainForm.AppInfo.SetBoolean("detailform", "gen_auto_run", dlg.AutoRun);

		if (dlg.DialogResult == DialogResult.OK)
		{
			this.Invoke(dlg.SelectedAction.ScriptEntry);
		}
	}
#endif

    void AddPinyin()
    {
        AddPinyin(this.PinyinCfgXml,
            true,
            PinyinStyle.None,
            "",
            this.DetailForm.MainForm.AutoSelPinyin);
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


    void Copy200fTo7x1a()
    {
        Copy200gfTo7xxa("f", "701");
    }

    void Copy200gTo7x2a()
    {
        Copy200gfTo7xxa("g", "702");
    }

    void Copy200gfTo7xxa(string strFromSubfield, string strToField)
    {
        Field field_200 = this.DetailForm.MarcEditor.Record.Fields.GetOneField("200", 0);
        if (field_200 == null)
        {
            MessageBox.Show(this.DetailForm, "200字段不存在");
            return;
        }

        SubfieldCollection subfields_200 = field_200.Subfields;

        Subfield subfield_f = subfields_200[strFromSubfield];

        if (subfield_f == null)
        {
            MessageBox.Show(this.DetailForm, "200$" + strFromSubfield + "不存在");
            return;
        }

        string strContent = subfield_f.Value;

        // 看看当前活动字段是不是701
        Field field_701 = null;

        field_701 = this.DetailForm.MarcEditor.FocusedField;
        if (field_701 != null)
        {
            if (field_701.Name != strToField)
                field_701 = null;
        }

        if (field_701 == null)
        {
            field_701 = this.DetailForm.MarcEditor.Record.Fields.GetOneField(strToField, 0);

            if (field_701 == null)
            {
                field_701 = this.DetailForm.MarcEditor.Record.Fields.Add(strToField, "  ", "", true);
            }
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

    void AddGcatAuthorNumber()
    {
        string strAuthor = "";

#if NO
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
#endif
        string strError = "";
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

        strError = "MARC记录中 700/710/720/701/711/702/712中均未发现包含汉字的 $a 子字段内容，无法获得著者字符串";
        goto ERROR1;
    FOUND:
        Debug.Assert(results.Count > 0, "");
        strAuthor = results[0];

        // BEGIN:
        string strGcatWebServiceUrl = this.DetailForm.MainForm.GcatServerUrl;   // "http://dp2003.com/dp2libraryws/gcat.asmx";

        string strNumber = "";

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

    void AddSjhmAuthorNumber()
    {
        string strError = "";

        string strAuthor = "";
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

        strError = "MARC记录中 700/710/720/701/711/702/712中均未发现包含汉字的 $a 子字段内容，无法获得著者字符串";
        goto ERROR1;
    FOUND:
        Debug.Assert(results.Count > 0, "");
        strAuthor = results[0];

        string strNumber = "";

        // 获得四角号码著者号
        // return:
        //      -1  error
        //      0   canceled
        //      1   succeed
        int nRet = GetSjhmAuthorNumber(
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

    // new 
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
            dlg.LibraryServerName = this.DetailForm.ServerName;

            dlg.Show();
            // dlg.WindowState = FormWindowState.Minimized;

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

        this.DetailForm.MarcEditor.Record.Fields.SetFirstSubfield("210", "a", strCity);
        this.DetailForm.MarcEditor.Record.Fields.SetFirstSubfield("210", "c", strName);


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
        {
            goto ERROR1;
        }

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

    // 维护210对照关系
    // 2008/10/17
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

    void Copy690aTo905d()
    {
        Copy690aTo905d("a", "905");
    }

    void Copy690aTo905d(string strFromSubfield, string strToField)
    {
        Field field_690 = this.DetailForm.MarcEditor.Record.Fields.GetOneField("690", 0);
        if (field_690 == null)
        {
            MessageBox.Show(this.DetailForm, "690字段不存在");
            return;
        }

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
}