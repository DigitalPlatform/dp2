
using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using System.Xml;
using static dp2LibraryApiTester.Utility;

namespace dp2LibraryApiTester.TestCase
{
    public static class TestCopyBiblioInfo
    {
        static string GetDataFileName(string name)
        {
            string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            dir = Path.Combine(dir, "data/testcopybiblioinfo");
            if (name == null)
                return dir;
            return Path.Combine(dir, name);
        }

        public static NormalResult TestAll(CancellationToken token)
        {
            NormalResult result = null;

            // *** 菜单选择
            string[] filenames = null;
            {
                var di = new DirectoryInfo(GetDataFileName(null));
                filenames = di.GetFiles("*.xml").Select(o => o.FullName).ToArray();

                if (filenames.Length > 1)
                {
                    var list = new List<string>();
                    list.Add("[全部]");
                    list.AddRange(filenames.Select(o => Path.GetFileName(o)));
                    bool temp = false;
                    var form = Application.OpenForms[0];
                    var ret = form.TryGet(() =>
                    {
                        return SelectDlg.GetSelect(form,
                        "title",
                        "请选择要执行的",
                        list.ToArray(),
                        0,
                        null,
                        ref temp,
                        form.Font);
                    });
                    if (ret == null)
                        return new NormalResult();
                    if (ret != "[全部]")
                    {
                        var fullname = filenames.Where(o => Path.GetFileName(o) == ret).FirstOrDefault();
                        filenames = new string[] { fullname };
                    }
                }
            }

            result = PrepareEnvironment();
            if (result.Value == -1) return result;

            token.ThrowIfCancellationRequested();

            {
                Utility.RunMany(
                    TestCopy,
                    filenames,
                    token);

                goto FINISH;
            }


            /*
            // *** Copy
            {
                Utility.RunMany(
TestCopy,
GetDataFileName("testcopy.xml"),
token);
            }
            */
#if REMOVED
            // *** Copy
            {
                var names = new string[] {
                    "权限不足",
                    "普通权限 只有 setbiblioinfo",
                    "普通权限 setbiblioinfo getbiblioinfo",
                    "存取定义 源:getbiblioinfo",
                    "存取定义 源:getbiblioinfo 目标:setbiblioinfo",
                    "存取定义 源:getbiblioinfo 目标:setbiblioinfo|getbiblioinfo",
                    "存取定义 源:getbiblioinfo 目标:setbiblioinfo|getbiblioinfo 源字段限制",
                    "存取定义 源:getbiblioinfo 目标:setbiblioinfo|getbiblioinfo 目标字段限制200",
                    "存取定义 源:getbiblioinfo 目标:setbiblioinfo|getbiblioinfo 目标字段限制300",
                    "存取定义 源:order",
                    "存取定义 目标:order",
                    "存取定义 源:order 目标:order",
                    "存取定义 源:order 目标:order 数据库 orderWork 角色"
};
                string[] styles = new string[] {
                    "subrecord",
                    "subrecord,s_o:getbiblioobject|getitemobject,t_o:setbiblioobject|setitemobject",
                    "subrecord,s_item:getiteminfo,s_o:getbiblioobject|getitemobject,t_item:setiteminfo,t_o:setbiblioobject|setitemobject",
                    "",
                    "change_while_copy",
                    "file",
                    "file,s_o:getobject,t_o:getobject|setobject",
                    "file,s_o:getbiblioobject,t_o:getbiblioobject|setbiblioobject",
                };

                Utility.RunMany(
TestCopy,
names,
styles,
token);
            }
#endif

            // *** CopyOverwrite
            {
                Utility.RunMany(
TestCopy,
GetDataFileName("testcopyoverwrite.xml"),
token);
            }

            /*
            // *** CopyOverwrite 附加 merge_style
            {
                var names = new string[] {
    "权限不足",
    "普通权限 只有 setbiblioinfo",
    "普通权限 setbiblioinfo getbiblioinfo",
    "存取定义 源:getbiblioinfo",
    "存取定义 源:getbiblioinfo 目标:setbiblioinfo",
    "存取定义 源:getbiblioinfo 目标:setbiblioinfo|getbiblioinfo",
    "存取定义 源:getbiblioinfo 目标:setbiblioinfo|getbiblioinfo 源字段限制",
    "存取定义 源:getbiblioinfo 目标:setbiblioinfo|getbiblioinfo 目标字段限制200",
    "存取定义 源:getbiblioinfo 目标:setbiblioinfo|getbiblioinfo 目标字段限制300",
                    "存取定义 源:order",
    "存取定义 目标:order",
    "存取定义 源:order 目标:order",
    "存取定义 源:order 目标:order 数据库 orderWork 角色"
};
                string[] styles = new string[] {
                    "",
                    "change_while_copy",
                    "file",
                    "file,objectright:setobject|getobject",
                    "file,objectright:setbiblioobject|getbiblioobject",
                };

                Utility.RunMany(
TestCopyOverwrite,
names,
styles,
token);
            }

            */

            token.ThrowIfCancellationRequested();

        FINISH:
            result = Finish();
            if (result.Value == -1) return result;

            return new NormalResult();
        }

        /*
new string[] {
    "权限不足",
    "普通权限 只有 setbiblioinfo",
    "普通权限 setbiblioinfo getbiblioinfo",
    "存取定义 源:getbiblioinfo",
    "存取定义 源:getbiblioinfo 目标:setbiblioinfo",
    "存取定义 源:getbiblioinfo 目标:setbiblioinfo|getbiblioinfo",
    "存取定义 源:getbiblioinfo 目标:setbiblioinfo|getbiblioinfo 源字段限制",
    "存取定义 源:getbiblioinfo 目标:setbiblioinfo|getbiblioinfo 目标字段限制200",
    "存取定义 源:getbiblioinfo 目标:setbiblioinfo|getbiblioinfo 目标字段限制300",
        "存取定义 源:order",
    "存取定义 目标:order",
    "存取定义 源:order 目标:order",
    "存取定义 源:order 目标:order 数据库 orderWork 角色"
};         * 
         * */
        // TODO: 对 CopyBiblioInfo() API strMergeStyle 参数的穷举测试
        // parameters:
        //      style   如果包含 change_while_copy 表示复制过程中会顺便改变目标 XML
        //              如果包含 s_o:xxx 表示 加上 xxx 作为账户对象相关权限中关于源库的部分
        //              如果包含 t_o:xxx 表示 加上 xxx 作为账户对象相关权限中关于目标库的部分
        //              如果包含 file，表示要在预先准备的 XML 记录中包含 dprms:file 元素
        //              如果包含 subrecord 表示要创建下属的册记录等
        //              如果包含 subrecord_file 要给下属的册记录等 XML 中添加 dprms:file 元素
        //      trigger 包含期望的结果的 XmlElement 对象
        //              	例如 <trigger ret='0' code='AccessDenied' info='*未包含*'/>

        static NormalResult TestCopy(
            XmlElement trigger,
            CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            //string name = Utility.GetProperty(trigger, "name");
            //string style = Utility.GetProperty(trigger, "style");

            DataModel.SetMessage($"===> 项目 复制书目记录 {Utility.GetTriggerPath(trigger)} ...\r\n{Utility.GetProperty(trigger, "comment")}{trigger.InnerText}");

            var biblio_parts = Utility.GetBiblioCreate(trigger);
            var file = StringUtil.IsInList("file", biblio_parts);
            var change_while_copy = Utility.GetProperty(trigger, "change_while_copy") != null;
            var merge_style = Utility.GetProperty(trigger, "merge_style");

            var subrecord = Utility.GetSubrecordCreate(trigger) != null;
            var subrecord_file = StringUtil.IsInList("file", Utility.GetSubrecordCreate(trigger));

            /*
            var file = StringUtil.IsInList("file", style);
            var s_o = StringUtil.GetParameterByPrefix(style, "s_o");
            var t_o = StringUtil.GetParameterByPrefix(style, "t_o");

            var change_while_copy = StringUtil.IsInList("change_while_copy", style);
            var merge_style = StringUtil.GetParameterByPrefix(style, "merge_style");

            var subrecord = StringUtil.IsInList("subrecord", style);
            var subrecord_file = StringUtil.IsInList("subrecord_file", style);
            var s_item = StringUtil.GetParameterByPrefix(style, "s_item");    // 关于源下级记录的权限
            var t_item = StringUtil.GetParameterByPrefix(style, "t_item");    // 关于目标下级记录的权限
            */

            // 覆盖风格。
            // null 为不覆盖，也就是追加。
            // "1" 表示第一种风格；"2" 表示第二种风格
            var overwrite_style = Utility.GetProperty(trigger, "overwrite");
            if (overwrite_style != null && overwrite_style != "1"
                && overwrite_style != "2")
                throw new ArgumentException($"overwrite 属性值 '{overwrite_style}' 不合法。应为 '1' '2' 之一");

            var origin_xml = GetXml();
            byte[] origin_timestamp = null;
            string path = "";
            string target_path = "";
            byte[] target_old_timestamp = null; // 目标位置记录刚创建时的时间戳
            byte[] target_new_timestamp = null; // 目标位置记录测试动作覆盖之后的时间戳

            string target_existing_xml = "";    // 目标位置预先写入的 XML 内容

            string changed_xml;
            // 修改了原有 XML 中的 title
            {
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(origin_xml);
                Utility.ChangeElementText(dom,
                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='200']/{http://dp2003.com/UNIMARC}:subfield[@code='a']",
                    // "//{http://dp2003.com/UNIMARC}:datafield[@tag='200']",
                    "changed title"
                    );
                changed_xml = dom.DocumentElement.OuterXml;
            }

            string GetXml()
            {
                var record = Utility.BuildBiblioRecord(
"title",
"");
                if (StringUtil.IsInList("998$z相同", biblio_parts))
                {
                    record.setFirstSubfield("998", "z", "_test_account");
                }
                else if (StringUtil.IsInList("998$z不相同", biblio_parts))
                {
                    record.setFirstSubfield("998", "z", "supervisor");
                }
                MarcUtil.Marc2Xml(record.Text,
                    "unimarc",
                    out string xml,
                    out _);
                if (file)
                {
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(xml);
                    XmlUtility.AddDprmsFileElement(dom, "1");
                    xml = dom.DocumentElement.OuterXml;
                }
                return xml;
            }

            void PrepareRecord(LibraryChannel c)
            {
                string s_path = _strBiblioDbName + "/?";
                string t_path = null;
                if (overwrite_style == "1")
                    t_path = _strTargetBiblioDbName + "/?";

                PrepareBiblioRecord(c,
                    ref s_path,
                    origin_xml,
                    out origin_timestamp,
                    ref t_path,
                    null,   // 令 target 记录 XML 和 orgin_xml 一致
                    out _);
                path = s_path;
                if (overwrite_style == "1")
                {
                    target_path = t_path;
                    target_existing_xml = Utility.GetBiblioXml(target_path);
                }

                if (subrecord)
                {
                    PrepareItemRecord(c,
                        path,
                        new string[]
                        {
                        "item",
                        //"order",
                        //"issue",
                        //"comment"
                        },
                        subrecord_file ? "file" : "");
                }
#if REMOVED
                var ret = c.SetBiblioInfo(null,
                    "new",
                    _strBiblioDbName + "/?",
                    "xml",
                    origin_xml,
                    null,
                    "",
                    out string output_path,
                    out origin_timestamp,
                    out string error);
                {
                    Utility.AssertResult(
                        "SetBiblioInfo()",
                        ret,
                        0,
                        c.ErrorCode,
                        ErrorCode.NoError,
                        error,
                        null,
                        false);

                    // GetBiblioInfo() 验证 XML 记录是否符合预期
                    var verify_ret = Utility.VerifyBiblioRecord(c,
                        output_path,
                        new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                        new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                        origin_xml,
                        origin_timestamp);
                    if (verify_ret.Value == -1)
                        throw new Exception($"(超级用户)创建测试用书目记录阶段出错: {verify_ret.ErrorInfo}");
                }
                path = output_path;
#endif
            }

            long DoCopy(LibraryChannel c,
                out string error,
                string new_path = null)
            {
                if (new_path == null)
                    new_path = target_path;
                if (string.IsNullOrEmpty(new_path))
                    new_path = _strTargetBiblioDbName + "/?";
                var ret = c.CopyBiblioInfo(null,
                    "copy",
                    path,
                    "xml",
                    "", // 
                    null,
                    new_path,
                    change_while_copy ? changed_xml : null,
                    merge_style,
                    out string output_biblio,
                    out string output_path,
                    out byte[] timestamp,
                    out error);
                if (ret != -1 && string.IsNullOrEmpty(output_path) == false)
                {
                    target_path = output_path;
                }
                target_new_timestamp = timestamp;
                return ret;
            }

            void DeleteRecord(LibraryChannel c)
            {
                if (string.IsNullOrEmpty(path) == false)
                {
                    byte[] last_timestamp = null;
                    int redo_count = 0;
                REDO:
                    var ret = c.SetBiblioInfo(null,
                        "delete",
                        path,
                        "xml",
                        "",
                        last_timestamp,
                        "",
                        out _,
                        out last_timestamp,
                        out string error);
                    if (ret == -1)
                    {
                        if (redo_count < 2
                            && c.ErrorCode == ErrorCode.TimestampMismatch)
                        {
                            redo_count++;
                            goto REDO;
                        }
                        throw new Exception($"清理阶段删除源书目记录 {path} 时出错: {error}");
                    }
                }

                if (string.IsNullOrEmpty(target_path) == false)
                {
                    byte[] last_timestamp = null;
                    int redo_count = 0;
                REDO:
                    var ret = c.SetBiblioInfo(null,
                        "delete",
                        target_path,
                        "xml",
                        "",
                        last_timestamp,
                        "",
                        out _,
                        out last_timestamp,
                        out string error);
                    if (ret == -1)
                    {
                        if (redo_count < 2
                            && c.ErrorCode == ErrorCode.TimestampMismatch)
                        {
                            redo_count++;
                            goto REDO;
                        }
                        throw new Exception($"清理阶段删除目标书目记录 {path} 时出错: {error}");
                    }
                }
            }

            // 断言源记录没有被修改。
            // parameters:
            //      new_timestamp   如果此参数不为 null，则表示比较时候用它而不是 origin_timestamp
            void AssertSourceNotChange(byte[] new_timestamp = null)
            {
                // 判断修改前后的 XML 没有任何变化。timestamp 也没有变化
                var channel = DataModel.GetChannel();
                try
                {
                    var verify_ret = Utility.VerifyBiblioRecord(channel,
                        path,
                        new string[] {
                                    "refID",
                                    "operations",
                                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']",
                        },
                        new string[] {
                                    "refID",
                                    "operations",
                                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']",
                        },
                        origin_xml,
                        new_timestamp == null ? origin_timestamp : new_timestamp);
                    if (verify_ret.Value == -1)
                        throw new Exception($"发现此时记录 {path} 已经被改变: {verify_ret.ErrorInfo}");
                }
                finally
                {
                    DataModel.ReturnChannel(channel);
                }
            }

            string save_old_role = null;

            return Utility._test(
    (super_channel) =>
    {
        var add_target_db_role = Utility.GetProperty(trigger, "add_target_db_role");
        // 修改数据库的角色
        if (string.IsNullOrEmpty(add_target_db_role) == false)
        {
            var change_role_ret = ManageHelper.ChangeDatabaseRole(
                super_channel,
                _strTargetBiblioDbName,
                (old_role) =>
                {
                    save_old_role = old_role;
                    StringUtil.SetInList(ref old_role, add_target_db_role, true);
                    DataModel.SetMessage($"为书目库 {_strTargetBiblioDbName} 添加 {add_target_db_role} 角色");
                    return old_role;
                });
            if (change_role_ret.Value == -1)
                throw new Exception($"修改书目库 '{_strTargetBiblioDbName}' 的角色发生错误: {change_role_ret.ErrorInfo}");
        }


        PrepareRecord(super_channel);



        var rights = Utility.GetProperty(trigger, "rights");
        var access = Utility.GetProperty(trigger, "access");

        return new UserInfo
        {
            UserName = "_test_account",
            Rights = CheckRights(rights),
            Access = ReplaceDbname(access),
        };
    },
    (channel, user) =>
    {
        long copy_ret = -1;
        {
            var ret = DoCopy(channel, out string error);
            if (overwrite_style != "2")
            {
                Utility.AssertResult(trigger,
                "CopyBiblioInfo()",
        ret,
        channel.ErrorCode,
        error);
            }
            else if (ret == -1)
                throw new ArgumentException($"为 overwrite 准备记录时出错: {error} code:{channel.ErrorCode}");

            if (ret == 0)
            {
                // 验证目标 XML 记录和源 XML 是否一样
                var verify_ret = Utility.VerifyBiblioRecord(channel,
                    target_path,
                    new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                    new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                    change_while_copy ? changed_xml : origin_xml,
                    target_new_timestamp);
                if (verify_ret.Value == -1)
                    return verify_ret;
            }
            copy_ret = ret;
        }

        if (overwrite_style == "2")
        {
            // 记载 XML 便于后面验证
            target_existing_xml = Utility.GetBiblioXml(target_path);

            // 覆盖式写入一次
            Debug.Assert(target_path != null);
            var ret = DoCopy(channel,
                out string error,
                target_path);
            Utility.AssertResult(trigger,
"CopyBiblioInfo()",
ret,
channel.ErrorCode,
error);
            copy_ret = ret;
        }

#if REMOVED
        var methods = Utility.GetProperty(trigger, "verify_target_biblio");
        if (methods == null)
        {
            if (copy_ret == -1)
                goto END1;
            methods = "=newly,timestamp";   // 不定义 verify_target_biblio 属性时的默认行为。如果不想校验，可以定义此属性值为 ""
        }
        if (string.IsNullOrEmpty(methods) == false)
        {
            VerifyTargetBiblio(
    channel,
    methods,
    target_path,
    target_existing_xml,
    change_while_copy ? changed_xml : origin_xml,
    target_new_timestamp);
        }
#endif
        var context = new VerifyContext
        {
            TargetPath = target_path,
            target_starting = origin_xml,
            target_copying = change_while_copy ? changed_xml : origin_xml,
            target_read = Utility.GetBiblioXml(target_path, false),
            target_old_timestamp = target_old_timestamp,
            target_new_timestamp = target_new_timestamp,
        };
        Utility.Verify(context, trigger);

    END1:
        DataModel.SetMessage("符合预期", "green");
        return new NormalResult();
    },
    // clean up
    (super_channel) =>
    {
        // 还原数据库的角色
        if (save_old_role != null)
        {
            var change_role_ret = ManageHelper.ChangeDatabaseRole(
                super_channel,
                _strTargetBiblioDbName,
                (old_role) =>
                {
                    return save_old_role;
                });
            if (change_role_ret.Value == -1)
                throw new Exception($"还原书目库 '{_strTargetBiblioDbName}' 的角色为 '{save_old_role}' 时发生错误: {change_role_ret.ErrorInfo}");
        }

        DeleteRecord(super_channel);
    },
    token);
            string name;
            switch (name)
            {
                case null:
                case "":
#if REMOVED
                case "权限不足":
                    return Utility._test(
                    (super_channel) =>
                    {
                        PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = ""
                        };
                    },
                    (channel, user) =>
                    {
                        Debug.Assert(string.IsNullOrEmpty(path) == false);

                        var ret = DoCopy(channel, out string error);
                        Utility.AssertResult(trigger,
    "CopyBiblioInfo()",
ret,
channel.ErrorCode,
error);
                        /*
                        {
                            Utility.AssertResult(
                "CopyBiblioInfo()",
                ret,
                -1,
                channel.ErrorCode,
                ErrorCode.AccessDenied,
                error);

                            // 判断修改前后的源没有任何变化。timestamp 也没有变化
                            AssertSourceNotChange();
                        }
                        */

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);
                case "普通权限 只有 setbiblioinfo":
                    return Utility._test(
                    (super_channel) =>
                    {
                        PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = BuildRights("setbiblioinfo", s_item, s_o,
                            "", t_item, t_o)
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoCopy(channel, out string error);
                        Utility.AssertResult(trigger,
"CopyBiblioInfo()",
ret,
channel.ErrorCode,
error);
                        /*
                        {
                            Utility.AssertResult(
        "CopyBiblioInfo()",
        ret,
        -1,
        channel.ErrorCode,
        ErrorCode.AccessDenied,
        error,
        "不具备");

                            // 判断修改前后的源没有任何变化。timestamp 也没有变化
                            AssertSourceNotChange();
                        }
                        */

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);
                case "普通权限 setbiblioinfo getbiblioinfo":
                    return Utility._test(
                    (super_channel) =>
                    {
                        PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = BuildRights("setbiblioinfo,getbiblioinfo", s_item, s_o,
                            "", t_item, t_o)
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoCopy(channel, out string error);
                        Utility.AssertResult(trigger,
                            "CopyBiblioInfo()",
ret,
channel.ErrorCode,
error);
                        if (ret == 0)
                        {
                            // 验证目标 XML 记录和源 XML 是否一样
                            var verify_ret = Utility.VerifyBiblioRecord(channel,
                                target_path,
                                new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                change_while_copy ? changed_xml : origin_xml,
                                target_timestamp);
                            if (verify_ret.Value == -1)
                                return verify_ret;
                        }
                        /*
                        {
                            if (subrecord && IsEmpty(s_item, t_item))
                                Utility.AssertResult(
"CopyBiblioInfo()",
ret,
-1,
channel.ErrorCode,
ErrorCode.AccessDenied,
error,
"书目记录带有下属的");
                            else if (file && IsEmpty(s_o, t_o))
                                Utility.AssertResult(
"CopyBiblioInfo()",
ret,
-1,
channel.ErrorCode,
ErrorCode.AccessDenied,
error,
"不足以完整读取源记录");
                            else
                            {
                                Utility.AssertResult(
            "CopyBiblioInfo()",
            ret,
            0,
            channel.ErrorCode,
            ErrorCode.NoError,
            error);
                                // 当前源记录 XML 一般没有必要验证

                                // 验证目标 XML 记录和源 XML 是否一样
                                var verify_ret = Utility.VerifyBiblioRecord(channel,
                                    target_path,
                                    new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                    new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                    change_while_copy ? changed_xml : origin_xml,
                                    target_timestamp);
                                if (verify_ret.Value == -1)
                                    return verify_ret;
                            }
                        }
                        */
                        DataModel.SetMessage("复制书目记录成功", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);

                case "存取定义 源:getbiblioinfo":
                    return Utility._test(
                    (super_channel) =>
                    {
                        PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = BuildAccess(
                                "getbiblioinfo", s_item, s_o,
                                "", t_item, t_o),
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoCopy(channel, out string error);
                        Utility.AssertResult(trigger,
                            "CopyBiblioInfo()",
ret,
channel.ErrorCode,
error);
                        /*
                        {
                            Utility.AssertResult(
        "CopyBiblioInfo()",
        ret,
        -1,
        channel.ErrorCode,
        ErrorCode.AccessDenied,
        error,
        "不具备");
                            // 判断修改前后的源没有任何变化。timestamp 也没有变化
                            AssertSourceNotChange();
                        }
                        */

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);
                case "存取定义 源:getbiblioinfo 目标:setbiblioinfo":
                    return Utility._test(
                    (super_channel) =>
                    {
                        PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = BuildAccess(
                                "getbiblioinfo", s_item, s_o,
                                "setbiblioinfo", t_item, t_o),
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoCopy(channel, out string error);
                        Utility.AssertResult(trigger,
    "CopyBiblioInfo()",
ret,
channel.ErrorCode,
error);
                        if (ret == 0)
                        {
                            // 验证目标 XML 记录和源 XML 是否一样
                            var verify_ret = Utility.VerifyBiblioRecord(channel,
                                target_path,
                                new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                change_while_copy ? changed_xml : origin_xml,
                                target_timestamp);
                            if (verify_ret.Value == -1)
                                return verify_ret;
                        }

#if REMOVED
                        {
                            if (subrecord && IsEmpty(s_item, t_item))
                                Utility.AssertResult(
"CopyBiblioInfo()",
ret,
-1,
channel.ErrorCode,
ErrorCode.AccessDenied,
error,
"书目记录带有下属的");
                            else if (file && IsEmpty(s_o, t_o))
                                Utility.AssertResult(
"CopyBiblioInfo()",
ret,
-1,
channel.ErrorCode,
ErrorCode.AccessDenied,
error,
"不足以完整读取");
                            /*
                            else if (file)
                                Utility.AssertResult(
"CopyBiblioInfo()",
ret,
-1,
channel.ErrorCode,
ErrorCode.SystemError,
error,
"写范围大于读范围");
                            */
                            else
                                Utility.AssertResult(
            "CopyBiblioInfo()",
            ret,
            0,
            channel.ErrorCode,
            ErrorCode.NoError,
            error);
                            // 判断修改前后的 XML 没有任何变化。timestamp 也没有变化
                            // AssertNotChange();
                        }
#endif

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);

                case "存取定义 源:getbiblioinfo 目标:setbiblioinfo|getbiblioinfo":
                    return Utility._test(
                    (super_channel) =>
                    {
                        PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = BuildAccess(
                                "getbiblioinfo", s_item, s_o,
                                "setbiblioinfo|getbiblioinfo", t_item, t_o),
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoCopy(channel, out string error);
                        {
                            if (subrecord && IsEmpty(s_item, t_item))
                                Utility.AssertResult(
"CopyBiblioInfo()",
ret,
-1,
channel.ErrorCode,
ErrorCode.AccessDenied,
error,
"书目记录带有下属的");
                            else if (file && IsEmpty(s_o, t_o))
                                Utility.AssertResult(
"CopyBiblioInfo()",
ret,
-1,
channel.ErrorCode,
ErrorCode.AccessDenied,
error,
"不足以完整读取源记录");
                            else
                            {
                                Utility.AssertResult(
        "CopyBiblioInfo()",
        ret,
        0,
        channel.ErrorCode,
        ErrorCode.NoError,
        error);

                                // GetBiblioInfo() 验证 XML 记录是否符合预期
                                var verify_ret = Utility.VerifyBiblioRecord(channel,
                                    target_path,
                                    new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                    new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                    change_while_copy ? changed_xml : origin_xml,
                                    target_timestamp);
                                if (verify_ret.Value == -1)
                                    return verify_ret;
                            }
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);


                case "存取定义 源:getbiblioinfo 目标:setbiblioinfo|getbiblioinfo 源字段限制":
                    return Utility._test(
                    (super_channel) =>
                    {
                        PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            // 权限限定只能读取 300 字段
                            Access = BuildAccess(
                                "getbiblioinfo=*(300)", s_item, s_o,
                                "setbiblioinfo|getbiblioinfo", t_item, t_o),

                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoCopy(channel, out string error);

                        {
                            Utility.AssertResult(
        "CopyBiblioInfo()",
        ret,
        -1,
        channel.ErrorCode,
        ErrorCode.AccessDenied,
        error,
        "不足以完整读取源");

                            // 判断修改前后的源没有任何变化。timestamp 也没有变化
                            AssertSourceNotChange();
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);

                case "存取定义 源:getbiblioinfo 目标:setbiblioinfo|getbiblioinfo 目标字段限制200":
                    return Utility._test(
                    (super_channel) =>
                    {
                        PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            // 权限限定只能修改 200 字段。源头记录中存在 200 字段
                            Access = BuildAccess(
                                "getbiblioinfo", s_item, s_o,
                                "setbiblioinfo=*(200)|getbiblioinfo", t_item, t_o),

                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoCopy(channel, out string error);

                        {
                            Utility.AssertResult(
        "CopyBiblioInfo()",
        ret,
        -1,
        channel.ErrorCode,
        ErrorCode.AccessDenied,
        error,
        "不足");

                            // 判断修改前后的源没有任何变化。timestamp 也没有变化
                            AssertSourceNotChange();
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);

                case "存取定义 源:getbiblioinfo 目标:setbiblioinfo|getbiblioinfo 目标字段限制300":
                    return Utility._test(
                    (super_channel) =>
                    {
                        PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            // 权限限定只能修改 300 字段。但源头记录中实际上并没有 300 字段，这样就等于目标位置什么字段都不能修改，等于全部字段都禁止了，这么一种效果
                            Access = BuildAccess(
                                "getbiblioinfo", s_item, s_o,
                                "setbiblioinfo=*(300)|getbiblioinfo", t_item, t_o),

                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoCopy(channel, out string error);

                        {
                            Utility.AssertResult(
        "CopyBiblioInfo()",
        ret,
        -1,
        channel.ErrorCode,
        file && IsEmpty(s_o, t_o) ? ErrorCode.AccessDenied : ErrorCode.SystemError,  // 这种情况是一种对可能误操作的报错。目前还不算做 AccessDenied
        error,
        file && IsEmpty(s_o, t_o) ? "不足以完整读取源记录" : "不存在任何字段");

                            // 判断修改前后的源没有任何变化。timestamp 也没有变化
                            AssertSourceNotChange();
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);


                case "存取定义 源:order":
                    return Utility._test(
                    (super_channel) =>
                    {
                        PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = BuildAccess(
                                "order", s_item, s_o,
                                "", t_item, t_o)
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoCopy(channel, out string error);

                        {
                            Utility.AssertResult(
        "CopyBiblioInfo()",
        ret,
        -1,
        channel.ErrorCode,
        ErrorCode.AccessDenied,
        error,
        "不具备");
                            // 判断修改前后的源没有任何变化。timestamp 也没有变化
                            AssertSourceNotChange();

                        }

                        DataModel.SetMessage("复制书目记录成功", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);
                case "存取定义 目标:order":
                    return Utility._test(
                    (super_channel) =>
                    {
                        PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = BuildAccess(
                                "", s_item, s_o,
                            "order", t_item, t_o)
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoCopy(channel, out string error);

                        {
                            Utility.AssertResult(
        "CopyBiblioInfo()",
        ret,
        -1,
        channel.ErrorCode,
        ErrorCode.AccessDenied,
        error,
        "不具备");
                            // 判断修改前后的源没有任何变化。timestamp 也没有变化
                            AssertSourceNotChange();

                        }

                        DataModel.SetMessage("复制书目记录成功", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);
                case "存取定义 源:order 目标:order":
                    return Utility._test(
                    (super_channel) =>
                    {
                        PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = BuildAccess(
                                "order", s_item, s_o,
                            "order", t_item, t_o)
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoCopy(channel, out string error);
                        if (subrecord && IsEmpty(s_item, t_item))
                            Utility.AssertResult(
"CopyBiblioInfo()",
ret,
-1,
channel.ErrorCode,
ErrorCode.AccessDenied,
error,
"书目记录带有下属的");
                        else if (file && IsEmpty(s_o, t_o))
                        {
                            Utility.AssertResult(
"CopyBiblioInfo()",
ret,
-1,
channel.ErrorCode,
ErrorCode.AccessDenied,
error,
"不足以完整读取源记录");
                        }
                        else
                        {
                            // 对于目标来说，追加记录不会引起问题。覆盖才会要求目标库必须是 orderWork 角色
                            Utility.AssertResult(
        "CopyBiblioInfo()",
        ret,
        0,
        channel.ErrorCode,
        ErrorCode.NoError,
        error);

                            // 验证目标
                            var verify_ret = Utility.VerifyBiblioRecord(channel,
                                target_path,
                                new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                change_while_copy ? changed_xml : origin_xml,
                                target_timestamp);
                            if (verify_ret.Value == -1)
                                return verify_ret;

                            // 覆盖式写入一次
                            Debug.Assert(target_path != null);
                            ret = DoCopy(channel,
                                out error,
                                target_path);

                            {
                                Utility.AssertResult(
    "CopyBiblioInfo()",
    ret,
    -1,
    channel.ErrorCode,
    ErrorCode.AccessDenied,
    error,
    "非工作库");
                            }

                        }


                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);


                case "存取定义 源:order 目标:order 数据库 orderWork 角色":
                    return Utility._test(
                    (super_channel) =>
                    {
                        // 修改数据库的角色
                        {
                            var change_role_ret = ManageHelper.ChangeDatabaseRole(
                                super_channel,
                                _strTargetBiblioDbName,
                                (old_role) =>
                                {
                                    StringUtil.SetInList(ref old_role, "orderWork", true);
                                    DataModel.SetMessage($"为书目库 {_strTargetBiblioDbName} 添加 orderWork 角色");
                                    return old_role;
                                });
                            if (change_role_ret.Value == -1)
                                throw new Exception($"修改书目库 '{_strTargetBiblioDbName}' 的角色发生错误: {change_role_ret.ErrorInfo}");
                        }

                        PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = BuildAccess(
                                "order", s_item, s_o,
                            "order", t_item, t_o)
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoCopy(channel, out string error);

                        if (subrecord && IsEmpty(s_item, t_item))
                            Utility.AssertResult(
"CopyBiblioInfo()",
ret,
-1,
channel.ErrorCode,
ErrorCode.AccessDenied,
error,
"书目记录带有下属的");
                        else if (file && IsEmpty(s_o, t_o))
                        {
                            Utility.AssertResult(
"CopyBiblioInfo()",
ret,
-1,
channel.ErrorCode,
ErrorCode.AccessDenied,
error,
"不足以完整读取源记录");
                        }
                        else
                        {
                            Utility.AssertResult(
        "CopyBiblioInfo()",
        ret,
        0,
        channel.ErrorCode,
        ErrorCode.NoError,
        error);

                            // 验证目标
                            var verify_ret = Utility.VerifyBiblioRecord(channel,
                                target_path,
                                new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                change_while_copy ? changed_xml : origin_xml,
                                target_timestamp);
                            if (verify_ret.Value == -1)
                                return verify_ret;

                            // 覆盖式写入一次
                            Debug.Assert(target_path != null);
                            ret = DoCopy(channel,
                                out error,
                                target_path);

                            {
                                Utility.AssertResult(
    "CopyBiblioInfo()",
    ret,
    0,
    channel.ErrorCode,
    ErrorCode.NoError,
    error);

                                // 验证目标
                                verify_ret = Utility.VerifyBiblioRecord(channel,
                                    target_path,
                                    new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                    new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                    change_while_copy ? changed_xml : origin_xml,
                                    target_timestamp);
                                if (verify_ret.Value == -1)
                                    return verify_ret;
                            }

                        }


                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        // 还原数据库的角色
                        {
                            var change_role_ret = ManageHelper.ChangeDatabaseRole(
                                super_channel,
                                _strTargetBiblioDbName,
                                (old_role) =>
                                {
                                    StringUtil.SetInList(ref old_role, "orderWork", false);
                                    return old_role;
                                });
                            if (change_role_ret.Value == -1)
                                throw new Exception($"还原书目库 '{_strTargetBiblioDbName}' 的角色发生错误: {change_role_ret.ErrorInfo}");
                        }

                        DeleteRecord(super_channel);
                    },
                    token);
#endif
                default:
                    throw new ArgumentException($"无法识别的 name '{name}'");
            }
        }


        static NormalResult TestCopyOverwrite(string name,
    string style,
    CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            DataModel.SetMessage($"===> 项目 复制书目记录覆盖目标之 {name} 情形。style={style} ...");

            var file = StringUtil.IsInList("file", style);
            var s_o = StringUtil.GetParameterByPrefix(style, "s_o");
            var t_o = StringUtil.GetParameterByPrefix(style, "t_o");

            var change_while_copy = StringUtil.IsInList("change_while_copy", style);
            var merge_style = StringUtil.GetParameterByPrefix(style, "merge_style");
            string s_item = "";
            string t_item = "";


            var origin_xml = GetXml();
            byte[] origin_timestamp = null;
            // byte[] last_timestamp = null;
            string path = "";
            string target_path = "";
            byte[] target_new_timestamp = null;
            byte[] target_old_timestamp = null;

            string changed_xml;
            // 修改了原有 XML 中的 title
            {
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(origin_xml);
                Utility.ChangeElementText(dom,
                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='200']/{http://dp2003.com/UNIMARC}:subfield[@code='a']",
                    // "//{http://dp2003.com/UNIMARC}:datafield[@tag='200']",
                    "changed title"
                    );
                changed_xml = dom.DocumentElement.OuterXml;
            }

            string GetXml()
            {
                var record = Utility.BuildBiblioRecord(
"title",
"");
                if (name.Contains("998$z相同"))
                {
                    record.setFirstSubfield("998", "z", "_test_account");
                }
                else if (name.Contains("998$z不相同"))
                {
                    record.setFirstSubfield("998", "z", "supervisor");
                }
                MarcUtil.Marc2Xml(record.Text,
                    "unimarc",
                    out string xml,
                    out _);
                if (file)
                {
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(xml);
                    XmlUtility.AddDprmsFileElement(dom, "1");
                    xml = dom.DocumentElement.OuterXml;
                }
                return xml;
            }

            void PrepareRecord(LibraryChannel c)
            {
                string s_path = _strBiblioDbName + "/?";
                string t_path = _strTargetBiblioDbName + "/?";
                PrepareBiblioRecord(c,
                    ref s_path,
                    origin_xml,
                    out origin_timestamp,
                    ref t_path,
                    null,
                    out target_old_timestamp);
                path = s_path;
                target_path = t_path;
#if REMOVED
                var ret = c.SetBiblioInfo(null,
                    "new",
                    _strBiblioDbName + "/?",
                    "xml",
                    origin_xml,
                    null,
                    "",
                    out string output_path,
                    out origin_timestamp,
                    out string error);
                {
                    Utility.AssertResult(
                        "SetBiblioInfo()",
                        ret,
                        0,
                        c.ErrorCode,
                        ErrorCode.NoError,
                        error,
                        null,
                        false);

                    // GetBiblioInfo() 验证 XML 记录是否符合预期
                    var verify_ret = Utility.VerifyBiblioRecord(c,
                        output_path,
                        new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                        new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                        origin_xml,
                        origin_timestamp);
                    if (verify_ret.Value == -1)
                        throw new Exception($"(超级用户)创建测试用书目记录阶段出错: {verify_ret.ErrorInfo}");
                }
                path = output_path;

                // 准备目标记录
                ret = c.SetBiblioInfo(null,
    "new",
    _strTargetBiblioDbName + "/?",
    "xml",
    origin_xml,
    null,
    "",
    out output_path,
    out target_old_timestamp,
    out error);
                {
                    Utility.AssertResult(
                        "SetBiblioInfo()",
                        ret,
                        0,
                        c.ErrorCode,
                        ErrorCode.NoError,
                        error,
                        null,
                        false);

                    // GetBiblioInfo() 验证 XML 记录是否符合预期
                    var verify_ret = Utility.VerifyBiblioRecord(c,
                        output_path,
                        new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                        new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                        origin_xml,
                        target_old_timestamp);
                    if (verify_ret.Value == -1)
                        throw new Exception($"(超级用户)创建测试用书目记录阶段出错: {verify_ret.ErrorInfo}");
                }
                target_path = output_path;
#endif
            }

            long DoCopy(LibraryChannel c,
                out string error)
            {
                var ret = c.CopyBiblioInfo(null,
                    "copy",
                    path,
                    "xml",
                    "", // 
                    null,
                    target_path,
                    change_while_copy ? changed_xml : null,
                    merge_style,
                    out string output_biblio,
                    out string output_path,
                    out byte[] timestamp,
                    out error);
                target_path = output_path;
                target_new_timestamp = timestamp;
                return ret;
            }

            void DeleteRecord(LibraryChannel c)
            {
                if (string.IsNullOrEmpty(path) == false)
                {
                    byte[] last_timestamp = null;
                    int redo_count = 0;
                REDO:
                    var ret = c.SetBiblioInfo(null,
                        "delete",
                        path,
                        "xml",
                        "",
                        last_timestamp,
                        "",
                        out _,
                        out last_timestamp,
                        out string error);
                    if (ret == -1)
                    {
                        if (redo_count < 2
                            && c.ErrorCode == ErrorCode.TimestampMismatch)
                        {
                            redo_count++;
                            goto REDO;
                        }
                        throw new Exception($"清理阶段删除书目记录 {path} 时出错: {error}");
                    }
                }

                if (string.IsNullOrEmpty(target_path) == false)
                {
                    byte[] last_timestamp = null;
                    int redo_count = 0;
                REDO:
                    var ret = c.SetBiblioInfo(null,
                        "delete",
                        target_path,
                        "xml",
                        "",
                        last_timestamp,
                        "",
                        out _,
                        out last_timestamp,
                        out string error);
                    if (ret == -1)
                    {
                        if (redo_count < 2
                            && c.ErrorCode == ErrorCode.TimestampMismatch)
                        {
                            redo_count++;
                            goto REDO;
                        }
                        throw new Exception($"清理阶段删除书目记录 {path} 时出错: {error}");
                    }
                }
            }

            // 断言源记录没有被修改。
            // parameters:
            //      new_timestamp   如果此参数不为 null，则表示比较时候用它而不是 origin_timestamp
            void AssertSourceNotChange(byte[] new_timestamp = null)
            {
                // 判断修改前后的 XML 没有任何变化。timestamp 也没有变化
                var channel = DataModel.GetChannel();
                try
                {
                    var verify_ret = Utility.VerifyBiblioRecord(channel,
                        path,
                        new string[] {
                                    "refID",
                                    "operations",
                                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']",
                        },
                        new string[] {
                                    "refID",
                                    "operations",
                                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']",
                        },
                        origin_xml,
                        new_timestamp == null ? origin_timestamp : new_timestamp);
                    if (verify_ret.Value == -1)
                        throw new Exception($"发现此时记录 {path} 已经被改变: {verify_ret.ErrorInfo}");
                }
                finally
                {
                    DataModel.ReturnChannel(channel);
                }
            }


            switch (name)
            {
                case "权限不足":
                    return Utility._test(
                    (super_channel) =>
                    {
                        PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = ""
                        };
                    },
                    (channel, user) =>
                    {
                        Debug.Assert(string.IsNullOrEmpty(path) == false);
                        var ret = DoCopy(channel, out string error);
                        {
                            Utility.AssertResult(
                "CopyBiblioInfo()",
                ret,
                -1,
                channel.ErrorCode,
                ErrorCode.AccessDenied,
                error);

                            // 判断修改前后的源没有任何变化。timestamp 也没有变化
                            AssertSourceNotChange();
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);
                case "普通权限 只有 setbiblioinfo":
                    return Utility._test(
                    (super_channel) =>
                    {
                        PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = BuildRights("setbiblioinfo", s_item, s_o,
                            "", t_item, t_o)
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoCopy(channel, out string error);
                        {
                            Utility.AssertResult(
        "CopyBiblioInfo()",
        ret,
        -1,
        channel.ErrorCode,
        ErrorCode.AccessDenied,
        error,
        "不具备");

                            // 判断修改前后的源没有任何变化。timestamp 也没有变化
                            AssertSourceNotChange();
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);
                case "普通权限 setbiblioinfo getbiblioinfo":
                    return Utility._test(
                    (super_channel) =>
                    {
                        PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = BuildRights("setbiblioinfo,getbiblioinfo", s_item, s_o,
                            "", t_item, t_o)
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoCopy(channel, out string error);
                        {
                            if (file && IsEmpty(s_o, t_o))
                                Utility.AssertResult(
"CopyBiblioInfo()",
ret,
-1,
channel.ErrorCode,
ErrorCode.AccessDenied,
error,
"不足以完整读取源记录");
                            else
                            {
                                Utility.AssertResult(
            "CopyBiblioInfo()",
            ret,
            0,
            channel.ErrorCode,
            ErrorCode.NoError,
            error);
                                // 当前源记录 XML 一般没有必要验证

                                // 验证目标 XML 记录和源 XML 是否一样
                                var verify_ret = Utility.VerifyBiblioRecord(channel,
                                    target_path,
                                    new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                    new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                    change_while_copy ? changed_xml : origin_xml,
                                    target_new_timestamp);
                                if (verify_ret.Value == -1)
                                    return verify_ret;
                            }
                        }

                        DataModel.SetMessage("复制书目记录成功", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);

                case "存取定义 源:getbiblioinfo":
                    return Utility._test(
                    (super_channel) =>
                    {
                        PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = BuildAccess(
                                "getbiblioinfo", s_item, s_o,
                            "", t_item, t_o),
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoCopy(channel, out string error);
                        {
                            Utility.AssertResult(
        "CopyBiblioInfo()",
        ret,
        -1,
        channel.ErrorCode,
        ErrorCode.AccessDenied,
        error,
        "不具备");
                            // 判断修改前后的源没有任何变化。timestamp 也没有变化
                            AssertSourceNotChange();
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);
                case "存取定义 源:getbiblioinfo 目标:setbiblioinfo":
                    return Utility._test(
                    (super_channel) =>
                    {
                        PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = BuildAccess(
                                "getbiblioinfo", s_item, s_o,
                            "setbiblioinfo", t_item, t_o),
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoCopy(channel, out string error);
                        {
                            if (file && IsEmpty(s_o, t_o))
                                Utility.AssertResult(
"CopyBiblioInfo()",
ret,
-1,
channel.ErrorCode,
ErrorCode.AccessDenied,
error,
"不足以完整读取");
                            else
                                Utility.AssertResult(
            "CopyBiblioInfo()",
            ret,
            0,
            channel.ErrorCode,
            ErrorCode.NoError,
            error);
                            // 判断修改前后的 XML 没有任何变化。timestamp 也没有变化
                            // AssertNotChange();
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);

                case "存取定义 源:getbiblioinfo 目标:setbiblioinfo|getbiblioinfo":
                    return Utility._test(
                    (super_channel) =>
                    {
                        PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = BuildAccess(
                                "getbiblioinfo", s_item, s_o,
                                "setbiblioinfo|getbiblioinfo", t_item, t_o),
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoCopy(channel, out string error);
                        {
                            if (file && IsEmpty(s_o, t_o))
                                Utility.AssertResult(
"CopyBiblioInfo()",
ret,
-1,
channel.ErrorCode,
ErrorCode.AccessDenied,
error,
"不足以完整读取源记录");
                            else
                            {
                                Utility.AssertResult(
        "CopyBiblioInfo()",
        ret,
        0,
        channel.ErrorCode,
        ErrorCode.NoError,
        error);

                                // GetBiblioInfo() 验证 XML 记录是否符合预期
                                var verify_ret = Utility.VerifyBiblioRecord(channel,
                                    target_path,
                                    new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                    new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                    change_while_copy ? changed_xml : origin_xml,
                                    target_new_timestamp);
                                if (verify_ret.Value == -1)
                                    return verify_ret;
                            }
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);


                case "存取定义 源:getbiblioinfo 目标:setbiblioinfo|getbiblioinfo 源字段限制":
                    return Utility._test(
                    (super_channel) =>
                    {
                        PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            // 权限限定只能读取 300 字段
                            Access = BuildAccess(
                                "getbiblioinfo=*(300)", s_item, s_o,
                            "setbiblioinfo|getbiblioinfo", t_item, t_o),

                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoCopy(channel, out string error);

                        {
                            Utility.AssertResult(
        "CopyBiblioInfo()",
        ret,
        -1,
        channel.ErrorCode,
        ErrorCode.AccessDenied,
        error,
        "不足以完整读取源");

                            // 判断修改前后的源没有任何变化。timestamp 也没有变化
                            AssertSourceNotChange();
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);

                case "存取定义 源:getbiblioinfo 目标:setbiblioinfo|getbiblioinfo 目标字段限制200":
                    return Utility._test(
                    (super_channel) =>
                    {
                        PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            // 权限限定只能修改 200 字段。源头记录中存在 200 字段
                            Access = BuildAccess(
                                "getbiblioinfo", s_item, s_o,
                                "setbiblioinfo=*(200)|getbiblioinfo", t_item, t_o),

                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoCopy(channel, out string error);

                        if (file && IsEmpty(s_o, t_o))
                            Utility.AssertResult(
"CopyBiblioInfo()",
ret,
-1,
channel.ErrorCode,
ErrorCode.AccessDenied,
error,
"不足以完整读取源记录");
                        else
                        {
                            // 变化的是 200 字段，正好可以写入
                            Utility.AssertResult(
        "CopyBiblioInfo()",
        ret,
        0,
        channel.ErrorCode,
        ErrorCode.NoError,
        error);

                            // 判断修改前后的源没有任何变化。timestamp 也没有变化
                            AssertSourceNotChange();
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);

                case "存取定义 源:getbiblioinfo 目标:setbiblioinfo|getbiblioinfo 目标字段限制300":
                    return Utility._test(
                    (super_channel) =>
                    {
                        PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            // 权限限定只能修改 300 字段。但源头记录中实际上并没有 300 字段，这样就等于目标位置什么字段都不能修改，等于全部字段都禁止了，这么一种效果
                            Access = BuildAccess(
                                "getbiblioinfo", s_item, s_o,
                                "setbiblioinfo=*(300)|getbiblioinfo", t_item, t_o),

                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoCopy(channel, out string error);
                        if (file && IsEmpty(s_o, t_o))
                            Utility.AssertResult(
"CopyBiblioInfo()",
ret,
-1,
channel.ErrorCode,
ErrorCode.AccessDenied,
error,
"不足以完整读取源记录");
                        else if (change_while_copy)
                            Utility.AssertResult(
"CopyBiblioInfo()",
ret,
-1,
channel.ErrorCode,
ErrorCode.AccessDenied,
error,
"不足以将 XML 记录完整写入目标记录");
                        else
                        {
                            Utility.AssertResult(
        "CopyBiblioInfo()",
        ret,
        0,
        channel.ErrorCode,
        ErrorCode.NoError,
        error);

                            // 判断修改前后的源没有任何变化。timestamp 也没有变化
                            AssertSourceNotChange();
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);


                case "存取定义 源:order":
                    return Utility._test(
                    (super_channel) =>
                    {
                        PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = BuildAccess(
                                "order", s_item, s_o,
                            "", t_item, t_o)
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoCopy(channel, out string error);

                        {
                            Utility.AssertResult(
        "CopyBiblioInfo()",
        ret,
        -1,
        channel.ErrorCode,
        ErrorCode.AccessDenied,
        error,
        "不具备");
                            // 判断修改前后的源没有任何变化。timestamp 也没有变化
                            AssertSourceNotChange();

                        }

                        DataModel.SetMessage("复制书目记录成功", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);
                case "存取定义 目标:order":
                    return Utility._test(
                    (super_channel) =>
                    {
                        PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = BuildAccess(
                                "", s_item, s_o,
                            "order", t_item, t_o)
                        };
                    },
                    (channel, user) =>
                    {
                        var ret = DoCopy(channel, out string error);

                        {
                            Utility.AssertResult(
        "CopyBiblioInfo()",
        ret,
        -1,
        channel.ErrorCode,
        ErrorCode.AccessDenied,
        error,
        "不具备");
                            // 判断修改前后的源没有任何变化。timestamp 也没有变化
                            AssertSourceNotChange();

                        }

                        DataModel.SetMessage("复制书目记录成功", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);
                case "存取定义 源:order 目标:order":
                    return Utility._test(
                    (super_channel) =>
                    {
                        PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = BuildAccess(
                                "order", s_item, s_o,
                                "order", t_item, t_o)
                        };
                    },
                    (channel, user) =>
                    {
                        // 覆盖式写入一次
                        Debug.Assert(target_path != null);
                        var ret = DoCopy(channel,
                            out string error);

                        if (file && IsEmpty(s_o, t_o))
                        {
                            Utility.AssertResult(
"CopyBiblioInfo()",
ret,
-1,
channel.ErrorCode,
ErrorCode.AccessDenied,
error,
"不足以完整读取源记录");
                        }
                        else
                        {
                            Utility.AssertResult(
"CopyBiblioInfo()",
ret,
-1,
channel.ErrorCode,
ErrorCode.AccessDenied,
error,
"非工作库");
                        }

                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        DeleteRecord(super_channel);
                    },
                    token);


                case "存取定义 源:order 目标:order 数据库 orderWork 角色":
                    return Utility._test(
                    (super_channel) =>
                    {
                        // 修改数据库的角色
                        {
                            var change_role_ret = ManageHelper.ChangeDatabaseRole(
                                super_channel,
                                _strTargetBiblioDbName,
                                (old_role) =>
                                {
                                    StringUtil.SetInList(ref old_role, "orderWork", true);
                                    DataModel.SetMessage($"为书目库 {_strTargetBiblioDbName} 添加 orderWork 角色");
                                    return old_role;
                                });
                            if (change_role_ret.Value == -1)
                                throw new Exception($"修改书目库 '{_strTargetBiblioDbName}' 的角色发生错误: {change_role_ret.ErrorInfo}");
                        }

                        PrepareRecord(super_channel);

                        return new UserInfo
                        {
                            UserName = "_test_account",
                            Rights = "",
                            Access = BuildAccess("order", s_item, s_o,
                            "order", t_item, t_o)
                        };
                    },
                    (channel, user) =>
                    {
                        // 覆盖式写入一次
                        Debug.Assert(target_path != null);
                        var ret = DoCopy(channel,
                            out string error);

                        if (file && IsEmpty(s_o, t_o))
                        {
                            Utility.AssertResult(
"CopyBiblioInfo()",
ret,
-1,
channel.ErrorCode,
ErrorCode.AccessDenied,
error,
"不足以完整读取源记录");
                        }
                        else
                        {
                            Utility.AssertResult(
"CopyBiblioInfo()",
ret,
0,
channel.ErrorCode,
ErrorCode.NoError,
error);

                            // 验证目标
                            var verify_ret = Utility.VerifyBiblioRecord(channel,
                                target_path,
                                new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                                change_while_copy ? changed_xml : origin_xml,
                                target_new_timestamp);
                            if (verify_ret.Value == -1)
                                return verify_ret;
                        }


                        DataModel.SetMessage("符合预期", "green");
                        return new NormalResult();
                    },
                    // clean up
                    (super_channel) =>
                    {
                        // 还原数据库的角色
                        {
                            var change_role_ret = ManageHelper.ChangeDatabaseRole(
                                super_channel,
                                _strTargetBiblioDbName,
                                (old_role) =>
                                {
                                    StringUtil.SetInList(ref old_role, "orderWork", false);
                                    return old_role;
                                });
                            if (change_role_ret.Value == -1)
                                throw new Exception($"还原书目库 '{_strTargetBiblioDbName}' 的角色发生错误: {change_role_ret.ErrorInfo}");
                        }

                        DeleteRecord(super_channel);
                    },
                    token);

                default:
                    throw new ArgumentException($"无法识别的 name '{name}'");
            }
        }


        static string _strBiblioDbName = "_测试用源";
        static string _strTargetBiblioDbName = "_测试用目标";
        static string _null_marc_header = null;

        public static NormalResult PrepareEnvironment()
        {
            string strError = "";
            int nRet = 0;
            long lRet = 0;

            try
            {
                {
                    // 利用默认账户进行准备操作
                    LibraryChannel channel = DataModel.GetChannel();
                    TimeSpan old_timeout = channel.Timeout;
                    channel.Timeout = TimeSpan.FromMinutes(10);

                    try
                    {
                        // 创建测试所需的书目库

                        // 如果测试用的书目库以前就存在，要先删除。
                        DataModel.SetMessage("正在删除测试用书目库 ...");
                        {
                            lRet = channel.ManageDatabase(
                null,
                "delete",
                _strBiblioDbName,    // strDatabaseNames,
                "",
                "",
                out _,
                out strError);
                            if (lRet == -1)
                            {
                                if (channel.ErrorCode != DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                                    goto ERROR1;
                            }
                        }

                        {
                            lRet = channel.ManageDatabase(
    null,
    "delete",
    _strTargetBiblioDbName,
    "",
    "",
    out _,
    out strError);
                            if (lRet == -1)
                            {
                                if (channel.ErrorCode != DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                                    goto ERROR1;
                            }
                        }

                        DataModel.SetMessage("正在创建测试用书目库 ...");
                        {
                            // 创建一个书目库
                            // parameters:
                            // return:
                            //      -1  出错
                            //      0   没有必要创建，或者操作者放弃创建。原因在 strError 中
                            //      1   成功创建
                            nRet = ManageHelper.CreateBiblioDatabase(
                                channel,
                                null,
                                _strBiblioDbName,
                                "series",   // _dbType == "issue" ? "series" : "book",
                                "unimarc",
                                "*",
                                "",
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                        }

                        {
                            nRet = ManageHelper.CreateBiblioDatabase(
        channel,
        null,
        _strTargetBiblioDbName,
        "series",
        "unimarc",
        "*",
        "",
        out strError);
                            if (nRet == -1)
                                goto ERROR1;
                        }

                        // 临时设置 不校验条码号
                        lRet = channel.SetSystemParameter(null,
                            "circulation",
                            "?VerifyBarcode",
                            "false",
                            out strError);
                        if (lRet == -1)
                            goto ERROR1;

                        lRet = channel.GetSystemParameter(null,
                            "utility",
                            "null_marc_header",
                            out string value,
                            out strError);
                        if (lRet == -1 || lRet == 0)
                            goto ERROR1;
                        _null_marc_header = value;

                        // 验证 supervisor 账户是否具备一些基本权限
                        var rights = channel.Rights;
                        if (StringUtil.IsInList("getobject,getbiblioinfo", rights) == false)
                            throw new Exception($"{channel.UserName} 账户缺乏 getobject 或 getbiblioobject 权限");
                        if (StringUtil.IsInList("setobject,setbiblioinfo", rights) == false)
                            throw new Exception($"{channel.UserName} 账户缺乏 setobject 或 setbiblioobject 权限");
                    }
                    finally
                    {
                        channel.Timeout = old_timeout;
                        DataModel.ReturnChannel(channel);
                    }
                }

                return new NormalResult();
            }
            catch (Exception ex)
            {
                strError = "PrepareEnvironment() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }

        ERROR1:
            DataModel.SetMessage($"PrepareEnvironment() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }


        public static NormalResult Finish()
        {
            string strError = "";

            LibraryChannel channel = DataModel.GetChannel();
            TimeSpan old_timeout = channel.Timeout;
            channel.Timeout = TimeSpan.FromMinutes(10);

            try
            {
                DataModel.SetMessage("正在删除测试用书目库 ...");
                {
                    long lRet = channel.ManageDatabase(
        null,
        "delete",
        _strBiblioDbName,
        "",
        "",
        out _,
        out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode != DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                            goto ERROR1;
                    }
                }

                {
                    long lRet = channel.ManageDatabase(
        null,
        "delete",
        _strTargetBiblioDbName,
        "",
        "",
        out _,
        out strError);
                    if (lRet == -1)
                    {
                        if (channel.ErrorCode != DigitalPlatform.LibraryClient.localhost.ErrorCode.NotFound)
                            goto ERROR1;
                    }
                }

                var ret = Utility.DeleteMemoryUsers(channel);
                if (ret.Value == -1)
                {
                    strError = ret.ErrorInfo;
                    goto ERROR1;
                }

                DataModel.SetMessage("结束");
                return new NormalResult();
            }
            catch (Exception ex)
            {
                strError = "Finish() Exception: " + ExceptionUtil.GetExceptionText(ex);
                goto ERROR1;
            }
            finally
            {
                channel.Timeout = old_timeout;
                DataModel.ReturnChannel(channel);
            }

        ERROR1:
            DataModel.SetMessage($"Finish() error: {strError}", "error");
            return new NormalResult
            {
                Value = -1,
                ErrorInfo = strError
            };
        }



        static bool IsEmpty(string source, string target)
        {
            return string.IsNullOrEmpty(source) && string.IsNullOrEmpty(target);
        }
        static string BuildRights(string source_base,
    string source_appendix,
    string source_appendix2,
    string target_base,
    string target_appendix,
    string target_appendix2)
        {
            string Combine(string a, string b, string c)
            {
                return _combine(_combine(a, b), c);
            }
            string _combine(string b, string a)
            {
                if (b != null && b.Contains("|"))
                {
                    b = b.Replace("|", ",");
                    // throw new ArgumentException($"'{b}'中不应该有竖线。请改用逗号");
                }
                if (a != null && a.Contains("|"))
                {
                    a = a.Replace("|", ",");
                    // throw new ArgumentException($"'{a}'中不应该有竖线。请改用逗号");
                }
                string r = b;
                if (string.IsNullOrEmpty(a) == false)
                {
                    if (string.IsNullOrEmpty(r) == false)
                        r += ",";
                    r += a;
                }
                return r;
            }

            string result = "";
            if (string.IsNullOrEmpty(source_base) == false
                || string.IsNullOrEmpty(source_appendix) == false
                || string.IsNullOrEmpty(source_appendix2) == false)
                result = Combine(source_base, source_appendix, source_appendix2);
            if (string.IsNullOrEmpty(target_base) == false
    || string.IsNullOrEmpty(target_appendix) == false
    || string.IsNullOrEmpty(target_appendix2) == false)
                result += "," + Combine(target_base, target_appendix, target_appendix2);

            return result;
        }

        static string BuildAccess(string source_base,
            string source_appendix,
            string source_appendix2,
            string target_base,
            string target_appendix,
            string target_appendix2)
        {
            string Combine(string a, string b, string c)
            {
                return _combine(_combine(a, b), c);
            }
            string _combine(string b, string a)
            {
                string r = b;
                if (string.IsNullOrEmpty(a) == false)
                {
                    if (string.IsNullOrEmpty(r) == false)
                        r += "|";
                    r += a;
                }
                return r;
            }

            string result = "";
            if (string.IsNullOrEmpty(source_base) == false
                || string.IsNullOrEmpty(source_appendix) == false
                || string.IsNullOrEmpty(source_appendix2) == false)
                result = _strBiblioDbName + ":" + Combine(source_base, source_appendix, source_appendix2);
            if (string.IsNullOrEmpty(target_base) == false
    || string.IsNullOrEmpty(target_appendix) == false
    || string.IsNullOrEmpty(target_appendix2) == false)
            {
                if (string.IsNullOrEmpty(result) == false)
                    result += ";";
                result += _strTargetBiblioDbName + ":" + Combine(target_base, target_appendix, target_appendix2);
            }

            return result;
        }

        // 创建源和目标两条书目记录
        static void PrepareBiblioRecord(LibraryChannel c,
            ref string source_path,
            string source_xml,
            out byte[] source_timestamp,
            ref string target_path,
            string target_xml,
            out byte[] target_timestamp)
        {
            source_timestamp = null;
            target_timestamp = null;
            if (string.IsNullOrEmpty(source_path) == false)
            {
                var ret = c.SetBiblioInfo(null,
                    "new",
                    source_path,
                    "xml",
                    source_xml,
                    null,
                    "",
                    out source_path,
                    out source_timestamp,
                    out string error);
                {
                    Utility.AssertResult(
                        "SetBiblioInfo()",
                        ret,
                        0,
                        c.ErrorCode,
                        ErrorCode.NoError,
                        error,
                        null,
                        false);

                    // GetBiblioInfo() 验证 XML 记录是否符合预期
                    var verify_ret = Utility.VerifyBiblioRecord(c,
                        source_path,
                        new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                        new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                        source_xml,
                        source_timestamp);
                    if (verify_ret.Value == -1)
                        throw new Exception($"(超级用户)创建测试用源书目记录阶段出错: {verify_ret.ErrorInfo}");
                }
            }

            // 准备目标记录
            if (string.IsNullOrEmpty(target_path) == false)
            {
                var ret = c.SetBiblioInfo(null,
    "new",
    target_path,
    "xml",
    string.IsNullOrEmpty(target_xml) ? source_xml : target_xml,
    null,
    "",
    out target_path,
    out target_timestamp,
    out string error);
                {
                    Utility.AssertResult(
                        "SetBiblioInfo()",
                        ret,
                        0,
                        c.ErrorCode,
                        ErrorCode.NoError,
                        error,
                        null,
                        false);

                    // GetBiblioInfo() 验证 XML 记录是否符合预期
                    var verify_ret = Utility.VerifyBiblioRecord(c,
                        target_path,
                        new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                        new string[] { "refID", "operations", "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']" },
                        string.IsNullOrEmpty(target_xml) ? source_xml : target_xml,
                        target_timestamp);
                    if (verify_ret.Value == -1)
                        throw new Exception($"(超级用户)创建测试用目标书目记录阶段出错: {verify_ret.ErrorInfo}");
                }
            }
        }

        static string _itemXml = @"<root>
<barcode>X0000001</barcode>
<location>阅览室</location>
<bookType>普通</bookType>
<price>CNY12.00</price>
</root>";

        static string _issueXml = @"<root>
<publishTime>20150101</publishTime>
<issue>1</issue>
<zong>1</zong>
<volume>1</volume>
</root>";

        static string _orderXml = @"<root>
<seller>qudao</seller>
<source>laiyuan</source>
<copy>1</copy>
<orderTime>Sun, 09 Apr 2017 00:00:00 +0800</orderTime>
<price>CNY12.00</price>
</root>";

        static string _commentXml = @"<root>
<type>书评</type>
<title>title</title>
<creator>R0000001</creator>
<content>comment</content>
<parent>612</parent>
</root>";

        // parameters:
        //      style   如果包含 file，表示要创建一个 dprms:file 元素
        static void PrepareItemRecord(LibraryChannel channel,
            string biblio_recpath,
            string[] dbtype_list,
            string style)
        {
            foreach (string db_type in dbtype_list)
            {
                string item_xml;
                switch (db_type)
                {
                    case "item":
                        item_xml = _itemXml;
                        break;
                    case "issue":
                        item_xml = _issueXml;
                        break;
                    case "order":
                        item_xml = _orderXml;
                        break;
                    case "comment":
                        item_xml = _commentXml;
                        break;
                    default:
                        throw new ArgumentException($"无法识别的 dbType '{db_type}'");
                }

                {
                    XmlDocument item_dom = new XmlDocument();
                    item_dom.LoadXml(item_xml);
                    if (StringUtil.IsInList("file", style))
                        XmlUtility.AddDprmsFileElement(item_dom, "1");
                    item_xml = item_dom.OuterXml;
                }


                List<EntityInfo> items = new List<EntityInfo>();
                EntityInfo item = new EntityInfo
                {
                    Action = "new",
                    NewRecPath = "",
                    NewRecord = item_xml,
                    OldTimestamp = null,
                    Style = "",
                };
                items.Add(item);
                var ret = SetItems(channel,
                    biblio_recpath,
                    db_type,
                    items.ToArray(),
                    out EntityInfo[] output_items,
                    out string strError);
                if (ret == -1)
                    throw new Exception(strError);
                var errors = output_items.Where(o => o.ErrorCode != ErrorCodeValue.NoError).Select(o => $"error:{o.ErrorInfo} code:{o.ErrorCode}").ToList();
                if (errors.Count > 0)
                    throw new Exception(StringUtil.MakePathList(errors, "; "));
            }
        }


        static long SetItems(LibraryChannel channel,
    string biblio_recpath,
    string _dbType,
    EntityInfo[] items,
    out EntityInfo[] output_items,
    out string strError)
        {
            long ret = -1;
            if (_dbType == "item")
                ret = channel.SetEntities(
        null,
        biblio_recpath,
        items,
        out output_items,
        out strError);
            else if (_dbType == "order")
                ret = channel.SetOrders(
null,
biblio_recpath,
items,
out output_items,
out strError);
            else if (_dbType == "issue")
                ret = channel.SetIssues(
null,
biblio_recpath,
items,
out output_items,
out strError);
            else if (_dbType == "comment")
                ret = channel.SetComments(
null,
biblio_recpath,
items,
out output_items,
out strError);
            else
                throw new ArgumentException($"未知的 _dbType: '{_dbType}'");

            /*
            if (output_items != null && output_items.Length > 0
                && output_items[0].NewTimestamp != null)
            {
                item_timestamp = output_items[0].NewTimestamp;
            }
            */

            if (string.IsNullOrEmpty(strError)
                && ret == -1)
                throw new ArgumentException($"SetItems() 中调用 SetXXX() API 的 strError 不该返回空");
            return ret;
        }

        static byte[] GetFirstTimestamp(EntityInfo[] output_items)
        {
            if (output_items != null && output_items.Length > 0
    && output_items[0].NewTimestamp != null)
            {
                return output_items[0].NewTimestamp;
            }

            return null;
        }

        public static string ReplaceDbname(string template)
        {
            if (template == null)
                return "";
            return template.Replace("{source}", _strBiblioDbName)
                .Replace("{target}", _strTargetBiblioDbName);
        }

        public static string CheckRights(string text)
        {
            if (text == null)
                return "";
            if (text.Contains("{") || text.Contains("}"))
                throw new ArgumentException($"普通权限字符串 '{text}' 中不允许出现花括号");
            return text;
        }

        // 根据参数调整要忽略的元素名字列表。
        // 目前支持的参数为: +997 -997 997 +refID -refID refID 等等。增减 names 中的元素名
        // 
        static string[] AdjustNames(string[] names,
            string[] parameters)
        {
            if (parameters == null)
                return names;
            var results = new List<string>();
            results.AddRange(names);
            foreach (var segment in parameters)
            {
                string action = "+";
                var name = segment;
                if (char.IsLetterOrDigit(segment.Substring(0, 1)[0]) == false)
                {
                    action = segment.Substring(0, 1);
                    name = segment.Substring(1);
                }

                // 替换一些 name 的简写形态
                if (name.Length == 3 && StringUtil.IsNumber(name))
                    name = $"//{{http://dp2003.com/UNIMARC}}:datafield[@tag='{name}']";

                if (action == "-")
                {
                    results.Remove(name);
                }
                else
                {
                    if (action != "+")
                        throw new ArgumentException($"参数 '{segment}' 中出现了无法识别的动作符号 '{action}'");
                    if (results.IndexOf(name) == -1)
                        results.Add(name);
                }
            }

            return results.ToArray();
        }

        // 执行属性 verify_target_biblio
        // 例: verify_target_biblio='=existing_origin'
        // 例: verify_target_biblio='=newly'
        // parameters:
        //      target_existing_xml 刚准备好的 target 位置的书目 XML 内容
        static void VerifyTargetBiblio(
            LibraryChannel channel,
            string methods,
            string target_path,
            string target_existing_xml,
            string target_newly_xml,
            byte[] target_timestamp)
        {
            var ignore_elements = new string[] {
                "refID",
                "operations",
                "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']"
            };

            string[] list = methods.Split(',');
            // 分离出 compare_ignore_fields 参数
            var ignore_fields_value = list.Where(o => o.StartsWith("compare_ignore_fields:"))
                .Select(o => o.Substring("compare_ignore_fields:".Length))
                .FirstOrDefault();

            // 分离出 compare_has_fields
            var has_fields_value = list.Where(o => o.StartsWith("compare_has_fields:"))
                .Select(o => o.Substring("compare_has_fields:".Length))
                .FirstOrDefault();

            ignore_elements = AdjustNames(ignore_elements,
                ignore_fields_value.Split('|'));

            foreach (var segment in list)
            {
                // 分离参数部分
                var parts = StringUtil.ParseTwoPart(segment, ":");
                var action = parts[0];
                var parameters = parts[1];

                if (action.StartsWith("compare_"))
                    continue;

                // 目标记录内容没有变化，和原先准备好时的内容一致
                if (action == "=existing_origin")
                {
                    var comparerable_target_existing_xml = Utility.RemoveElements(target_existing_xml, ignore_elements);

                    var verify_ret = Utility.VerifyBiblioRecord(channel,
    target_path,
    null,
    /*
    new string[] {
                                    "refID",
                                    "operations",
                                    "//{http://dp2003.com/UNIMARC}:datafield[@tag='997']",
    },
    */
    ignore_elements,
    comparerable_target_existing_xml,
    null);
                    if (verify_ret.Value == -1)
                        throw new Exception($"verify_target_biblio 属性值 {action} 执行验证目标记录失败: {verify_ret.ErrorInfo}");
                }

                // 目标记录内容和测试意图保存的新内容一致
                if (action == "=newly")
                {
                    var comparerable_target_xml = Utility.RemoveElements(target_newly_xml, ignore_elements);

                    var verify_ret = Utility.VerifyBiblioRecord(channel,
    target_path,
    null,
    ignore_elements,
    comparerable_target_xml,
    null);
                    if (verify_ret.Value == -1)
                        throw new Exception($"verify_target_biblio 属性值 {action} 执行验证目标记录失败: {verify_ret.ErrorInfo}");
                }

                if (action == "timestamp")
                {
                    var verify_ret = Utility.VerifyBiblioTimestamp(channel,
    target_path,
    target_timestamp);
                    if (verify_ret.Value == -1)
                        throw new Exception($"verify_target_biblio 属性值 {action} 执行验证目标记录失败: {verify_ret.ErrorInfo}");
                }
            }
        }
    }
}
