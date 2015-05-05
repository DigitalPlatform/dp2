// dp2circulation 西文期刊 MARC 编目自动创建数据C#脚本程序
// 最后修改时间 2012/9/22

// 1) 2012/9/22 CreateMenu()函数增加了对BindingForm的处理

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
    public void CreateMenu(object sender, GenerateDataEventArgs e)
    {
        ScriptActionCollection actions = new ScriptActionCollection();

        if (sender is MarcEditor || sender == null)
        {
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

    public override void CreateCallNumber(object sender,
    GenerateDataEventArgs e)
    {
        base.CreateCallNumber(sender, e);
    }
}